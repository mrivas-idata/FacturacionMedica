#!/usr/bin/env python3
"""Jarvis de iData Chile — interfaz de línea de comandos.

Uso:
    python jarvis.py                      # conversación interactiva
    python jarvis.py "resumen del día"    # una sola pregunta
"""

from __future__ import annotations

import sys

import anthropic

from jarvis_core.agente import Jarvis

AYUDA = """\
Comandos:  /pendientes  /clientes  /ley  /nuevo (borra la conversación)  /ayuda  /salir
Ejemplos:  "Dame el resumen del día"
           "Cotiza los servicios 1, 5 y 6 de la Ley 21.719 con la UF de hoy"
           "Redacta el correo de seguimiento a Agrovial"
           "Prepara el guion de la próxima sesión en Radio Ritmo sobre phishing"
"""

ATAJOS = {
    "/pendientes": "Muéstrame mis pendientes abiertos agrupados por área y dime cuál atacar primero.",
    "/clientes": "Muéstrame la cartera de clientes y prospectos y qué seguimiento corresponde a cada uno.",
    "/ley": "¿Cuántos días quedan para la vigencia de la Ley 21.719 y qué implica para la venta de iData?",
}


def _escribir(texto: str) -> None:
    sys.stdout.write(texto)
    sys.stdout.flush()


def _herramienta(nombre: str, _entrada: dict) -> None:
    sys.stdout.write(f"\n  · {nombre}…\n")
    sys.stdout.flush()


def _preguntar(jarvis: Jarvis, texto: str) -> None:
    try:
        jarvis.preguntar(texto)
    except anthropic.AuthenticationError:
        print("\n[Error] Credenciales inválidas. Define ANTHROPIC_API_KEY (ver README).")
    except TypeError as exc:
        if "authentication" not in str(exc):
            raise
        print("\n[Error] Falta la credencial de Claude. Define ANTHROPIC_API_KEY o ejecuta "
              "`ant auth login` (ver README).")
    except anthropic.RateLimitError:
        print("\n[Error] Límite de uso alcanzado. Intenta en unos segundos.")
    except anthropic.APIStatusError as exc:
        print(f"\n[Error de la API {exc.status_code}] {exc.message}")
    except anthropic.APIConnectionError:
        print("\n[Error] Sin conexión con la API de Claude.")
    print()


def main() -> None:
    jarvis = Jarvis(al_escribir=_escribir, al_usar_herramienta=_herramienta)

    if len(sys.argv) > 1:
        _preguntar(jarvis, " ".join(sys.argv[1:]))
        return

    print("JARVIS · iData Chile — a sus órdenes, Marco.  (/ayuda para ver comandos)\n")
    while True:
        try:
            texto = input("Marco › ").strip()
        except (EOFError, KeyboardInterrupt):
            print("\nHasta pronto.")
            return
        if not texto:
            continue
        if texto in ("/salir", "/exit"):
            print("Hasta pronto.")
            return
        if texto == "/ayuda":
            print(AYUDA)
            continue
        if texto == "/nuevo":
            jarvis.reiniciar()
            print("Conversación reiniciada.\n")
            continue
        sys.stdout.write("Jarvis › ")
        _preguntar(jarvis, ATAJOS.get(texto, texto))
        print()


if __name__ == "__main__":
    main()
