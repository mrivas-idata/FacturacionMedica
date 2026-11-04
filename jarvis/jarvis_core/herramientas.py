"""Herramientas locales de Jarvis.

Cada herramienta es una función pura de Python que recibe un dict validado y devuelve
un dict serializable. Lo que es exacto (cálculos, fechas, registros) se resuelve aquí,
en código determinista; el modelo solo interpreta y redacta.
"""

from __future__ import annotations

import json
import os
import re
from datetime import date, datetime
from pathlib import Path
from typing import Any, Callable

RAIZ = Path(__file__).resolve().parent.parent
VIGENCIA_LEY_21719 = date(2026, 12, 1)

# Catálogo de la propuesta Ley 21.719 (UF mínimo, UF máximo, plazo).
CATALOGO_LEY_21719: dict[int, dict[str, Any]] = {
    1: {"servicio": "Diagnóstico y levantamiento inicial", "uf_min": 35, "uf_max": 50, "plazo": "2–3 semanas"},
    2: {"servicio": "Pseudonimización en ambientes de desarrollo y testing", "uf_min": 80, "uf_max": 120, "plazo": "4–6 semanas"},
    3: {"servicio": "Anonimización para analítica", "uf_min": 60, "uf_max": 90, "plazo": "3–5 semanas"},
    4: {"servicio": "Cifrado diferenciado según sensibilidad", "uf_min": 70, "uf_max": 110, "plazo": "4–6 semanas"},
    5: {"servicio": "Controles de acceso y auditoría (RBAC + logging)", "uf_min": 40, "uf_max": 60, "plazo": "2–3 semanas"},
    6: {"servicio": "Documentación, evidencias y RAT", "uf_min": 25, "uf_max": 35, "plazo": "2 semanas"},
    7: {"servicio": "Capacitación al equipo técnico y DPO", "uf_min": 15, "uf_max": 20, "plazo": "1 semana"},
}

AREAS_PENDIENTES = ("idata", "consalud", "proyectos", "personal")
ESTADOS_CLIENTE = ("prospecto", "activo", "entregado", "inactivo")


class ErrorHerramienta(Exception):
    """Error esperado de una herramienta: se devuelve al modelo como tool_result con is_error."""


# --------------------------------------------------------------------------- almacenamiento

def _dir_datos() -> Path:
    return Path(os.environ.get("JARVIS_DATA_DIR", RAIZ / "data"))


def _leer(nombre: str) -> list[dict[str, Any]]:
    ruta = _dir_datos() / nombre
    if not ruta.exists():
        return []
    return json.loads(ruta.read_text(encoding="utf-8"))


def _escribir(nombre: str, datos: list[dict[str, Any]]) -> None:
    ruta = _dir_datos() / nombre
    ruta.parent.mkdir(parents=True, exist_ok=True)
    tmp = ruta.with_suffix(".tmp")
    tmp.write_text(json.dumps(datos, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    tmp.replace(ruta)


def _hoy() -> date:
    forzada = os.environ.get("JARVIS_FECHA_HOY")
    return date.fromisoformat(forzada) if forzada else date.today()


def _validar_fecha(valor: str | None) -> str | None:
    if valor in (None, ""):
        return None
    try:
        return date.fromisoformat(valor).isoformat()
    except ValueError as exc:
        raise ErrorHerramienta(f"Fecha inválida '{valor}'. Usa el formato AAAA-MM-DD.") from exc


# --------------------------------------------------------------------------- pendientes

def listar_pendientes(area: str | None = None, incluir_terminados: bool = False) -> dict[str, Any]:
    if area and area not in AREAS_PENDIENTES:
        raise ErrorHerramienta(f"Área desconocida '{area}'. Opciones: {', '.join(AREAS_PENDIENTES)}.")
    hoy = _hoy()
    items = []
    for p in _leer("pendientes.json"):
        if area and p["area"] != area:
            continue
        if p["hecho"] and not incluir_terminados:
            continue
        item = dict(p)
        if p.get("fecha_limite") and not p["hecho"]:
            dias = (date.fromisoformat(p["fecha_limite"]) - hoy).days
            item["dias_restantes"] = dias
            item["vencido"] = dias < 0
        items.append(item)
    return {"total": len(items), "pendientes": items}


def agregar_pendiente(area: str, tarea: str, fecha_limite: str | None = None) -> dict[str, Any]:
    if area not in AREAS_PENDIENTES:
        raise ErrorHerramienta(f"Área desconocida '{area}'. Opciones: {', '.join(AREAS_PENDIENTES)}.")
    tarea = tarea.strip()
    if not tarea:
        raise ErrorHerramienta("La tarea no puede estar vacía.")
    datos = _leer("pendientes.json")
    nuevo = {
        "id": max((p["id"] for p in datos), default=0) + 1,
        "area": area,
        "tarea": tarea,
        "fecha_limite": _validar_fecha(fecha_limite),
        "hecho": False,
    }
    datos.append(nuevo)
    _escribir("pendientes.json", datos)
    return {"agregado": nuevo}


def completar_pendiente(id: int) -> dict[str, Any]:
    datos = _leer("pendientes.json")
    for p in datos:
        if p["id"] == id:
            p["hecho"] = True
            p["terminado_el"] = _hoy().isoformat()
            _escribir("pendientes.json", datos)
            return {"completado": p}
    raise ErrorHerramienta(f"No existe un pendiente con id {id}.")


# --------------------------------------------------------------------------- clientes

def listar_clientes(estado: str | None = None) -> dict[str, Any]:
    clientes = [c for c in _leer("clientes.json") if not estado or c["estado"] == estado]
    return {"total": len(clientes), "clientes": clientes}


def registrar_cliente(nombre: str, servicio: str, estado: str, notas: str = "") -> dict[str, Any]:
    if estado not in ESTADOS_CLIENTE:
        raise ErrorHerramienta(f"Estado inválido '{estado}'. Opciones: {', '.join(ESTADOS_CLIENTE)}.")
    datos = _leer("clientes.json")
    fecha = _hoy().isoformat()
    for c in datos:
        if c["nombre"].casefold() == nombre.strip().casefold():
            c["servicio"] = servicio
            c["estado"] = estado
            if notas:
                c["notas"] = f"{c.get('notas', '')} [{fecha}] {notas}".strip()
            _escribir("clientes.json", datos)
            return {"actualizado": c}
    nuevo = {"nombre": nombre.strip(), "servicio": servicio, "estado": estado,
             "notas": f"[{fecha}] {notas}".strip() if notas else ""}
    datos.append(nuevo)
    _escribir("clientes.json", datos)
    return {"creado": nuevo}


# --------------------------------------------------------------------------- Ley 21.719

def cotizar_ley_21719(servicios: list[int], valor_uf_clp: float | None = None) -> dict[str, Any]:
    if not servicios:
        raise ErrorHerramienta("Indica al menos un servicio (números 1 a 7).")
    invalidos = sorted({s for s in servicios if s not in CATALOGO_LEY_21719})
    if invalidos:
        raise ErrorHerramienta(f"Servicios inexistentes: {invalidos}. El catálogo va del 1 al 7.")
    if valor_uf_clp is not None and valor_uf_clp <= 0:
        raise ErrorHerramienta("El valor de la UF debe ser positivo.")

    elegidos = sorted(set(servicios))
    detalle = [{"n": n, **CATALOGO_LEY_21719[n]} for n in elegidos]
    uf_min = sum(d["uf_min"] for d in detalle)
    uf_max = sum(d["uf_max"] for d in detalle)
    resultado: dict[str, Any] = {
        "modalidad": "Llave en mano" if len(elegidos) == len(CATALOGO_LEY_21719) else "Modular por servicio",
        "detalle": detalle,
        "total_uf": {"min": uf_min, "max": uf_max},
        "hitos_pago": [
            {"hito": "Inicio", "porcentaje": 40, "uf_min": round(uf_min * 0.4, 2), "uf_max": round(uf_max * 0.4, 2)},
            {"hito": "Avance 50 %", "porcentaje": 40, "uf_min": round(uf_min * 0.4, 2), "uf_max": round(uf_max * 0.4, 2)},
            {"hito": "Cierre", "porcentaje": 20, "uf_min": round(uf_min * 0.2, 2), "uf_max": round(uf_max * 0.2, 2)},
        ],
        "nota": "Valores referenciales; precio final tras la reunión técnica de levantamiento. UF del día de firma.",
    }
    if valor_uf_clp:
        resultado["total_clp"] = {"min": round(uf_min * valor_uf_clp), "max": round(uf_max * valor_uf_clp),
                                  "valor_uf_usado": valor_uf_clp}
    return resultado


def dias_para_ley_21719() -> dict[str, Any]:
    hoy = _hoy()
    dias = (VIGENCIA_LEY_21719 - hoy).days
    return {
        "hoy": hoy.isoformat(),
        "vigencia": VIGENCIA_LEY_21719.isoformat(),
        "dias_restantes": dias,
        "semanas_restantes": round(dias / 7, 1),
        "en_vigor": dias <= 0,
        "alcanza_llave_en_mano": dias >= 14 * 7,
    }


# --------------------------------------------------------------------------- notas

def guardar_nota(titulo: str, contenido: str) -> dict[str, Any]:
    slug = re.sub(r"[^a-z0-9]+", "-", titulo.lower()).strip("-")[:60] or "nota"
    carpeta = _dir_datos() / "notas"
    carpeta.mkdir(parents=True, exist_ok=True)
    ruta = carpeta / f"{datetime.now():%Y%m%d-%H%M%S}-{slug}.md"
    ruta.write_text(f"# {titulo}\n\n{contenido}\n", encoding="utf-8")
    return {"guardado_en": str(ruta)}


# --------------------------------------------------------------------------- registro

def _obj(props: dict[str, Any], required: list[str]) -> dict[str, Any]:
    return {"type": "object", "properties": props, "required": required, "additionalProperties": False}


_AREA = {"type": "string", "enum": list(AREAS_PENDIENTES)}

DEFINICIONES: list[dict[str, Any]] = [
    {
        "name": "listar_pendientes",
        "description": "Lista los pendientes de Marco (iData, Consalud, proyectos, personal). Úsala antes de "
                       "responder sobre tareas, prioridades o el resumen del día.",
        "input_schema": _obj({
            "area": {**_AREA, "description": "Filtra por área. Omítela para ver todas."},
            "incluir_terminados": {"type": "boolean", "description": "Incluye tareas ya hechas."},
        }, []),
    },
    {
        "name": "agregar_pendiente",
        "description": "Agrega una tarea a la lista de pendientes.",
        "input_schema": _obj({
            "area": _AREA,
            "tarea": {"type": "string"},
            "fecha_limite": {"type": "string", "description": "AAAA-MM-DD, opcional."},
        }, ["area", "tarea"]),
    },
    {
        "name": "completar_pendiente",
        "description": "Marca un pendiente como terminado por su id (obtén el id con listar_pendientes).",
        "input_schema": _obj({"id": {"type": "integer"}}, ["id"]),
    },
    {
        "name": "listar_clientes",
        "description": "Lista clientes y prospectos de iData Chile con su estado y notas.",
        "input_schema": _obj({"estado": {"type": "string", "enum": list(ESTADOS_CLIENTE)}}, []),
    },
    {
        "name": "registrar_cliente",
        "description": "Crea o actualiza un cliente/prospecto. Las notas se agregan con fecha al historial.",
        "input_schema": _obj({
            "nombre": {"type": "string"},
            "servicio": {"type": "string"},
            "estado": {"type": "string", "enum": list(ESTADOS_CLIENTE)},
            "notas": {"type": "string"},
        }, ["nombre", "servicio", "estado"]),
    },
    {
        "name": "cotizar_ley_21719",
        "description": "Calcula una cotización referencial de los servicios Ley 21.719 (1–7 del catálogo) en UF, "
                       "con hitos de pago 40/40/20 y, si se entrega el valor de la UF, el total en CLP. "
                       "Usa siempre esta herramienta para cifras; no sumes de memoria.",
        "input_schema": _obj({
            "servicios": {"type": "array", "items": {"type": "integer", "minimum": 1, "maximum": 7}},
            "valor_uf_clp": {"type": "number", "description": "Valor de la UF en pesos, si se conoce."},
        }, ["servicios"]),
    },
    {
        "name": "dias_para_ley_21719",
        "description": "Días y semanas que faltan para la vigencia plena de la Ley 21.719 (01/12/2026).",
        "input_schema": _obj({}, []),
    },
    {
        "name": "guardar_nota",
        "description": "Guarda un borrador, minuta o documento en Markdown en la carpeta de notas de Jarvis.",
        "input_schema": _obj({"titulo": {"type": "string"}, "contenido": {"type": "string"}},
                             ["titulo", "contenido"]),
    },
]

FUNCIONES: dict[str, Callable[..., dict[str, Any]]] = {
    "listar_pendientes": listar_pendientes,
    "agregar_pendiente": agregar_pendiente,
    "completar_pendiente": completar_pendiente,
    "listar_clientes": listar_clientes,
    "registrar_cliente": registrar_cliente,
    "cotizar_ley_21719": cotizar_ley_21719,
    "dias_para_ley_21719": dias_para_ley_21719,
    "guardar_nota": guardar_nota,
}

_TIPOS = {"string": str, "integer": int, "number": (int, float), "boolean": bool, "array": list}


def _validar(nombre: str, entrada: Any) -> dict[str, Any]:
    esquema = next(d["input_schema"] for d in DEFINICIONES if d["name"] == nombre)
    if not isinstance(entrada, dict):
        raise ErrorHerramienta("La entrada debe ser un objeto JSON.")
    props = esquema["properties"]
    extra = set(entrada) - set(props)
    if extra:
        raise ErrorHerramienta(f"Parámetros desconocidos: {sorted(extra)}.")
    faltan = [r for r in esquema["required"] if r not in entrada]
    if faltan:
        raise ErrorHerramienta(f"Faltan parámetros obligatorios: {faltan}.")
    for clave, valor in entrada.items():
        tipo = props[clave].get("type")
        esperado = _TIPOS.get(tipo)
        es_bool_falso = tipo in ("integer", "number") and isinstance(valor, bool)
        if esperado and (not isinstance(valor, esperado) or es_bool_falso):
            raise ErrorHerramienta(f"'{clave}' debe ser de tipo {tipo}.")
        if "enum" in props[clave] and valor not in props[clave]["enum"]:
            raise ErrorHerramienta(f"'{clave}' debe ser uno de {props[clave]['enum']}.")
    return entrada


def ejecutar(nombre: str, entrada: Any) -> tuple[str, bool]:
    """Ejecuta una herramienta. Devuelve (contenido_json, es_error)."""
    if nombre not in FUNCIONES:
        return json.dumps({"error": f"Herramienta desconocida: {nombre}"}, ensure_ascii=False), True
    try:
        resultado = FUNCIONES[nombre](**_validar(nombre, entrada))
        return json.dumps(resultado, ensure_ascii=False), False
    except ErrorHerramienta as exc:
        return json.dumps({"error": str(exc)}, ensure_ascii=False), True
