import json
import shutil
import sys
from pathlib import Path
from types import SimpleNamespace

import pytest

RAIZ = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(RAIZ))

from jarvis_core import herramientas  # noqa: E402
from jarvis_core.agente import Jarvis  # noqa: E402


@pytest.fixture(autouse=True)
def datos_temporales(tmp_path, monkeypatch):
    for f in ("pendientes.json", "clientes.json"):
        shutil.copy(RAIZ / "data" / f, tmp_path / f)
    monkeypatch.setenv("JARVIS_DATA_DIR", str(tmp_path))
    monkeypatch.setenv("JARVIS_FECHA_HOY", "2026-11-04")
    return tmp_path


def _ok(nombre, entrada):
    contenido, error = herramientas.ejecutar(nombre, entrada)
    assert not error, contenido
    return json.loads(contenido)


def test_cotizacion_llave_en_mano_coincide_con_propuesta():
    r = _ok("cotizar_ley_21719", {"servicios": [1, 2, 3, 4, 5, 6, 7], "valor_uf_clp": 40000})
    assert r["total_uf"] == {"min": 325, "max": 485}
    assert r["modalidad"] == "Llave en mano"
    assert [h["porcentaje"] for h in r["hitos_pago"]] == [40, 40, 20]
    assert r["total_clp"]["min"] == 13_000_000


def test_cotizacion_modular_y_errores():
    r = _ok("cotizar_ley_21719", {"servicios": [1, 1, 6]})
    assert r["total_uf"] == {"min": 60, "max": 85}
    assert r["modalidad"] == "Modular por servicio"
    _, error = herramientas.ejecutar("cotizar_ley_21719", {"servicios": [9]})
    assert error


def test_dias_para_ley():
    r = _ok("dias_para_ley_21719", {})
    assert r["dias_restantes"] == 27
    assert r["alcanza_llave_en_mano"] is False


def test_ciclo_de_pendientes():
    abiertos = _ok("listar_pendientes", {})["total"]
    nuevo = _ok("agregar_pendiente", {"area": "idata", "tarea": "Llamar a Agrovial",
                                      "fecha_limite": "2026-11-01"})["agregado"]
    lista = _ok("listar_pendientes", {"area": "idata"})["pendientes"]
    assert any(p["id"] == nuevo["id"] and p["vencido"] for p in lista)
    _ok("completar_pendiente", {"id": nuevo["id"]})
    assert _ok("listar_pendientes", {})["total"] == abiertos


def test_validacion_de_entradas():
    for nombre, entrada in [
        ("agregar_pendiente", {"area": "marte", "tarea": "x"}),
        ("agregar_pendiente", {"area": "idata", "tarea": "x", "fecha_limite": "mañana"}),
        ("completar_pendiente", {"id": "1"}),
        ("completar_pendiente", {"id": 999}),
        ("listar_clientes", {"otro": 1}),
        ("no_existe", {}),
    ]:
        _, error = herramientas.ejecutar(nombre, entrada)
        assert error, (nombre, entrada)


def test_registrar_cliente_actualiza_existente():
    total = _ok("listar_clientes", {})["total"]
    r = _ok("registrar_cliente", {"nombre": "agrovial", "servicio": "Ley 21.719", "estado": "activo",
                                  "notas": "Aceptó la propuesta"})
    assert "actualizado" in r and "Aceptó" in r["actualizado"]["notas"]
    assert _ok("listar_clientes", {})["total"] == total


def test_guardar_nota(datos_temporales):
    r = _ok("guardar_nota", {"titulo": "Correo Tecnigen", "contenido": "Hola"})
    assert Path(r["guardado_en"]).read_text(encoding="utf-8").startswith("# Correo Tecnigen")


# ---------------------------------------------------------------- bucle del agente (API simulada)

def _bloque_texto(t):
    return SimpleNamespace(type="text", text=t)


class _StreamFalso:
    def __init__(self, mensaje):
        self.mensaje = mensaje

    def __enter__(self):
        return self

    def __exit__(self, *a):
        return False

    @property
    def text_stream(self):
        return iter(b.text for b in self.mensaje.content if b.type == "text")

    def get_final_message(self):
        return self.mensaje


class _ClienteFalso:
    def __init__(self, respuestas):
        self.respuestas = list(respuestas)
        self.llamadas = []
        self.beta = SimpleNamespace(messages=SimpleNamespace(stream=self._stream))

    def _stream(self, **kwargs):
        self.llamadas.append({**kwargs, "messages": list(kwargs["messages"])})
        return _StreamFalso(self.respuestas.pop(0))


def test_agente_ejecuta_herramientas_y_responde():
    uso = SimpleNamespace(type="tool_use", id="t1", name="dias_para_ley_21719", input={})
    cliente = _ClienteFalso([
        SimpleNamespace(stop_reason="tool_use", content=[_bloque_texto("Reviso. "), uso]),
        SimpleNamespace(stop_reason="end_turn", content=[_bloque_texto("Quedan 27 días.")]),
    ])
    usadas = []
    jarvis = Jarvis(cliente=cliente, al_usar_herramienta=lambda n, e: usadas.append(n))

    assert jarvis.preguntar("¿Cuánto falta para la ley?") == "Reviso. Quedan 27 días."
    assert usadas == ["dias_para_ley_21719"]

    segunda = cliente.llamadas[1]
    resultado = segunda["messages"][-1]["content"][0]
    assert resultado["type"] == "tool_result" and resultado["tool_use_id"] == "t1"
    assert json.loads(resultado["content"])["dias_restantes"] == 27
    assert segunda["system"][0]["cache_control"] == {"type": "ephemeral"}
    assert segunda["fallbacks"] == "default"
    assert "base_de_conocimiento" in segunda["system"][0]["text"]


def test_agente_maneja_rechazo():
    cliente = _ClienteFalso([SimpleNamespace(stop_reason="refusal", content=[])])
    assert "no pudo completar" in Jarvis(cliente=cliente).preguntar("algo")
