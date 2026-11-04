# JARVIS · iData Chile

Asistente ejecutivo de **iData Chile SpA**, construido sobre Claude. Tiene presente la empresa
completa (servicios, oferta Ley 21.719, clientes, productos, tono y principios) y administra tus
pendientes y tu cartera de clientes.

## Qué sabe hacer

| Capacidad | Cómo |
|---|---|
| Conoce iData de memoria | `conocimiento/idata_chile.md` se carga en cada sesión (con caché, para que sea barato) |
| Pendientes (iData, Consalud, proyectos, personal) | listar, agregar con fecha límite, completar · detecta vencidos |
| Cartera de clientes | listar por estado, crear/actualizar con historial fechado |
| Cotizaciones Ley 21.719 | cálculo exacto en UF y CLP de los servicios 1–7, hitos 40/40/20 |
| Cuenta regresiva Ley 21.719 | días/semanas al 01/12/2026 y si alcanza un proyecto llave en mano |
| Información actual | búsqueda web (valor UF, noticias, normativa), con fuente |
| Redacción | propuestas, correos, informes, guiones de radio y charlas con el estilo iData |
| Borradores | `guardar_nota` los deja en `data/notas/` en Markdown |

Principio de diseño (el mismo que iData recomienda a sus clientes): **el código calcula, el modelo
interpreta, Marco valida.** Las cifras y los registros salen de funciones deterministas; nada se
envía a un cliente sin tu revisión.

## Instalación

```bash
cd jarvis
python3 -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
export ANTHROPIC_API_KEY="sk-ant-..."     # o: ant auth login
```

## Uso

```bash
python jarvis.py                                  # conversación
python jarvis.py "Dame el resumen del día"        # una sola consulta
```

Comandos dentro de la conversación: `/pendientes`, `/clientes`, `/ley`, `/nuevo`, `/ayuda`, `/salir`.

Ejemplos:

- "¿Qué tengo pendiente esta semana y qué ataco primero?"
- "Cotiza diagnóstico, controles de acceso y documentación para un cliente con SQL Server 2019, con la UF de hoy."
- "Redacta el correo de seguimiento a Agrovial y guárdalo como nota."
- "Agrega: preparar propuesta Tecnigen para el 2026-11-10."
- "Prepara el guion de 15 minutos para Radio Ritmo sobre phishing por WhatsApp."
- "Moreno y Cía aprobó migrar Trello a API: actualiza el cliente."

## Configuración

| Variable | Defecto | Uso |
|---|---|---|
| `ANTHROPIC_API_KEY` | — | Credencial de la API de Claude |
| `JARVIS_MODELO` | `claude-opus-5-5` | Modelo |
| `JARVIS_ESFUERZO` | `medium` | `low` para respuestas rápidas, `high` para análisis/propuestas |
| `JARVIS_DATA_DIR` | `jarvis/data` | Dónde viven pendientes, clientes y notas |

## Mantenerlo al día

- **Conocimiento**: edita `conocimiento/idata_chile.md` (nuevos servicios, precios, clientes, productos).
- **Pendientes y clientes**: pídeselo a Jarvis o edita `data/*.json`.
- **Nuevas herramientas**: agrega la función, su esquema en `DEFINICIONES` y su entrada en
  `FUNCIONES` dentro de `jarvis_core/herramientas.py`.

## Pruebas

```bash
pip install pytest && python -m pytest -q tests
```

Las pruebas cubren las herramientas y el bucle del agente con una API simulada (no gastan tokens).

## Estructura

```
jarvis/
├── jarvis.py                    # CLI
├── jarvis_core/
│   ├── agente.py                # bucle con Claude: streaming, herramientas, caché, fallback
│   └── herramientas.py          # pendientes, clientes, cotizador, notas
├── conocimiento/idata_chile.md  # todo lo que Jarvis sabe de iData
├── data/                        # pendientes.json, clientes.json, notas/
└── tests/
```
