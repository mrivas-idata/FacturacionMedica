"""Bucle de conversación de Jarvis sobre la API de Claude."""

from __future__ import annotations

import os
from datetime import date
from pathlib import Path
from typing import Any, Callable

import anthropic

from . import herramientas

RAIZ = Path(__file__).resolve().parent.parent
MODELO = os.environ.get("JARVIS_MODELO", "claude-opus-5-5")
ESFUERZO = os.environ.get("JARVIS_ESFUERZO", "medium")

PERSONALIDAD = """\
Eres Jarvis, el asistente ejecutivo de iData Chile SpA y mano derecha de Marco Rivas, su fundador.
Hablas en español de Chile, con trato cercano pero profesional, y vas directo al punto.

Qué haces:
- Llevas presente todo el contexto de la empresa (base de conocimiento abajo): servicios, la oferta
  Ley 21.719, clientes, proyectos, productos, tono de comunicación y principios de trabajo.
- Administras los pendientes y la cartera de clientes con tus herramientas. Antes de hablar de
  tareas, prioridades o clientes, consulta las herramientas: la base de conocimiento puede estar
  desactualizada y las herramientas son la fuente vigente.
- Redactas propuestas, cotizaciones, correos, informes, guiones de radio y material de charlas con
  el estilo de iData: lenguaje simple, analogías de la casa, resumen ejecutivo primero, tablas para
  datos, próximos pasos numerados, firma "M. Rivas".
- Asesoras en ciberseguridad y cumplimiento de la Ley 21.719 con criterio técnico real
  (Oracle 11g SE, SQL Server, Google Workspace, EDR, MFA, automatización con IA).

Reglas:
- Toda cifra de cotización sale de la herramienta cotizar_ley_21719; nunca sumes de memoria.
- Si necesitas datos actuales (valor de la UF, noticias, normativa reciente), usa la búsqueda web y
  cita la fuente.
- Nada se envía a un cliente sin que Marco lo revise: tú preparas el borrador y él valida.
- No mezcles información confidencial de un cliente en material para otro.
- Si algo no lo sabes o no está en tus datos, dilo y propón cómo averiguarlo.
- Al cerrar un análisis, sugiere la siguiente acción concreta.
"""


def cargar_sistema() -> list[dict[str, Any]]:
    conocimiento = (RAIZ / "conocimiento" / "idata_chile.md").read_text(encoding="utf-8")
    texto = f"{PERSONALIDAD}\n<base_de_conocimiento>\n{conocimiento}\n</base_de_conocimiento>"
    # El sistema es estable entre turnos: se cachea (herramientas + sistema) para abaratar cada turno.
    return [{"type": "text", "text": texto, "cache_control": {"type": "ephemeral"}}]


HERRAMIENTAS: list[dict[str, Any]] = [
    *herramientas.DEFINICIONES,
    {"type": "web_search_20260209", "name": "web_search", "max_uses": 5,
     "user_location": {"type": "approximate", "country": "CL", "city": "Santiago",
                       "timezone": "America/Santiago"}},
]


class Jarvis:
    def __init__(self, cliente: anthropic.Anthropic | None = None,
                 al_escribir: Callable[[str], None] | None = None,
                 al_usar_herramienta: Callable[[str, dict[str, Any]], None] | None = None) -> None:
        self.cliente = cliente or anthropic.Anthropic()
        self.sistema = cargar_sistema()
        self.mensajes: list[Any] = []
        self.al_escribir = al_escribir or (lambda _t: None)
        self.al_usar_herramienta = al_usar_herramienta or (lambda _n, _e: None)

    def reiniciar(self) -> None:
        self.mensajes = []

    def preguntar(self, texto: str, max_vueltas: int = 15) -> str:
        """Envía un mensaje de Marco y ejecuta herramientas hasta obtener la respuesta final."""
        # La fecha va en el turno (no en el sistema) para no romper la caché del prefijo.
        self.mensajes.append({"role": "user", "content": f"[Hoy es {date.today().isoformat()}]\n{texto}"})
        respuesta_final: list[str] = []

        for _ in range(max_vueltas):
            with self.cliente.beta.messages.stream(
                model=MODELO,
                max_tokens=64000,
                system=self.sistema,
                tools=HERRAMIENTAS,
                messages=self.mensajes,
                thinking={"type": "adaptive"},
                output_config={"effort": ESFUERZO},
                betas=["server-side-fallback-2026-07-01"],
                fallbacks="default",
            ) as stream:
                for fragmento in stream.text_stream:
                    self.al_escribir(fragmento)
                mensaje = stream.get_final_message()

            self.mensajes.append({"role": "assistant", "content": mensaje.content})
            respuesta_final.extend(b.text for b in mensaje.content if b.type == "text")

            if mensaje.stop_reason == "refusal":
                aviso = "\n[Jarvis no pudo completar esta solicitud por las políticas del modelo.]"
                self.al_escribir(aviso)
                respuesta_final.append(aviso)
                break
            if mensaje.stop_reason == "pause_turn":
                continue  # búsqueda web larga: el servidor continúa el turno
            if mensaje.stop_reason == "max_tokens":
                aviso = "\n[Respuesta truncada por longitud.]"
                self.al_escribir(aviso)
                respuesta_final.append(aviso)
                break
            if mensaje.stop_reason != "tool_use":
                break

            resultados = []
            for bloque in mensaje.content:
                if bloque.type != "tool_use":
                    continue
                self.al_usar_herramienta(bloque.name, bloque.input)
                contenido, es_error = herramientas.ejecutar(bloque.name, bloque.input)
                resultados.append({"type": "tool_result", "tool_use_id": bloque.id,
                                   "content": contenido, "is_error": es_error})
            # Todos los resultados en un solo mensaje de usuario.
            self.mensajes.append({"role": "user", "content": resultados})
        else:
            aviso = "\n[Se alcanzó el máximo de pasos con herramientas en este turno.]"
            self.al_escribir(aviso)
            respuesta_final.append(aviso)

        return "".join(respuesta_final)
