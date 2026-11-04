# Base de conocimiento — iData Chile SpA

> Fuente: documentos internos de iData Chile (guiones, propuestas, informes y la lista de pendientes).
> Actualiza este archivo cuando cambie algo importante: Jarvis lo carga completo en cada sesión.

## 1. La empresa

| Campo | Detalle |
|---|---|
| Razón social | IDATA CHILE SPA |
| RUT | 78.264.877-2 |
| Oficina | Providencia #1208, Of. 1603, Santiago |
| Sitio web | www.idatachile.com (WhatsApp directo desde el sitio) |
| Correo corporativo | mrivas@idatachile.com |
| Fundador / responsable | Marco Rivas (firma como "M. Rivas") |
| Lema | "Ciberseguridad para todos" |
| Posicionamiento | Empresa chilena de ciberseguridad, diseñada para la realidad chilena. "No venimos a instalar algo y desaparecer: acompañamos a su organización de forma permanente." |

Marco además trabaja en **Consalud** (desarrollo de software). Pendientes allí: terminar el
servicio de Bonos y publicar un package. Perfil técnico: ingeniero informático (USACH / UTEM),
jefe de proyectos senior, líder técnico full-stack (.NET, Angular, React, Flutter), APIs REST,
pruebas con Postman/SoapUI/JMeter, pipelines CI/CD. Un ejemplo de su trabajo .NET es el
proyecto FacturacionMedica de este mismo repositorio (API Clean Architecture, .NET 10, ApiKey,
tests unitarios).

## 2. Líneas de servicio (las 5 áreas)

| Área | Qué hacemos |
|---|---|
| Diagnóstico | Evaluamos riesgos, vulnerabilidades, accesos y respaldos. Le decimos exactamente cómo está la empresa. Es siempre **el primer paso** y la primera reunión es sin compromiso. |
| Controles de Seguridad | MFA, EDR, control de accesos, políticas de contraseñas, protección de equipos. |
| Gobierno de Ciberseguridad | Políticas internas, matriz de riesgos, protocolo de incidentes, plan de continuidad. |
| Protección de Datos Personales | Cumplimiento Ley 21.719: inventario y clasificación de datos, políticas de privacidad, evaluaciones de impacto, RAT. |
| Monitoreo Continuo | Evaluaciones periódicas, reportes KPI, recomendaciones y mejora continua. |

Servicios adicionales que iData ya ejecuta:

- **Automatización con IA para estudios y pymes** (ej. Abogados Moreno y Cía): triage de correo,
  alertas de audiencias, seguimiento de remates, propuestas de reuniones, notas de cobro, informes.
- **Migración de correo a Google Workspace** (ej. iProyecta).
- **Sitios web** para clientes (ej. sitio web de Raúl, ya publicado) y su informe periódico.
- **Charlas y difusión**: Radio Ritmo (San Vicente), charla gratuita de Protección de Datos
  (junio 2026), sesiones temáticas (ej. Sesión 11: Doble factor y ClaveÚnica).

## 3. Ley 21.719 — Protección de Datos Personales (oferta estrella)

- Publicada en el Diario Oficial el **13/12/2024**; **vigencia plena el 01/12/2026**.
- Reemplaza la Ley 19.628 (1999); se alinea con el GDPR; crea la **Agencia de Protección de Datos Personales**.
- Sanciones: leves hasta 5.000 UTM; graves hasta 10.000 UTM; gravísimas hasta 20.000 UTM o 4 % de
  los ingresos anuales si hay reincidencia.
- Obligaciones técnicas clave: cifrado en tránsito y reposo, pseudonimización/anonimización,
  mínimo privilegio, Registro de Actividades de Tratamiento (RAT), **notificación de brechas en 72 h**.

### Catálogo de la propuesta (valores referenciales, UF)

| N° | Servicio | Plazo | UF mín | UF máx |
|---|---|---|---|---|
| 1 | Diagnóstico y levantamiento inicial (inventario de datos, flujos, clasificación, brechas) | 2–3 sem | 35 | 50 |
| 2 | Pseudonimización en ambientes de desarrollo y testing | 4–6 sem | 80 | 120 |
| 3 | Anonimización para analítica (k-anonimato, generalización, supresión) | 3–5 sem | 60 | 90 |
| 4 | Cifrado diferenciado según sensibilidad (TDE, TLS 1.2/1.3, KMS, rotación) | 4–6 sem | 70 | 110 |
| 5 | Controles de acceso y auditoría (RBAC + logging) | 2–3 sem | 40 | 60 |
| 6 | Documentación, evidencias y RAT | 2 sem | 25 | 35 |
| 7 | Capacitación al equipo técnico y DPO | 1 sem | 15 | 20 |
| | **Total llave en mano** | 14–22 sem | **325** | **485** |

- Plataformas cubiertas: Oracle 11g SE (sin Data Masking Pack → scripting PL/SQL, FGA, cifrado por
  columna) y SQL Server 2016/2019/2022 (DDM, Always Encrypted, TDE, RLS, SQL Server Audit,
  clasificación de datos).
- Modalidades: **Llave en mano**, **Modular por servicio**, **Retainer** mensual post-implementación.
- Pago sugerido: **40 % inicio / 40 % al 50 % de avance / 20 % cierre**.
- Condiciones: UF del día de firma; NDA previo; trabajo remoto seguro; licencias de terceros aparte;
  validez de la propuesta 30 días corridos.
- Próximos pasos tipo: (1) reunión técnica sin costo → (2) revisión de ambientes →
  (3) propuesta definitiva + contrato + NDA → (4) inicio; primer entregable en 2–3 semanas.

### Kit documental de cumplimiento (plantillas existentes en Drive)

01 Política de Tratamiento de Datos · 03 Registro de Actividades (RAT) · 04 Protocolo de Brechas 72 h ·
06 Contrato de Servicios (datoseguro) · 08 Certificado de Cumplimiento · 11 Acta de Capacitación y
Asistencia · 12 Checklist de Inspección de la Agencia · 13 Contrato DPO Externo ·
Anexo A Formulario de Evaluación de Brecha · Anexo B Notificación a la Agencia ·
Anexo C Notificación a Titulares · Registro de Incidentes (planilla) · NDA iData Chile.

## 4. Productos propios

| Producto | Estado |
|---|---|
| **iDataSeguro** | Publicado como landing page y como SaaS. |
| **MiSGSI** | En desarrollo (Sistema de Gestión de Seguridad de la Información). |
| **iReservo** | App en desarrollo. |

## 5. Clientes y proyectos

| Cliente | Proyecto | Estado / notas |
|---|---|---|
| Abogados Moreno y Cía | Análisis y mejora de 5 automatizaciones (informe 05/08/2026) | 13 hallazgos (1 crítico: Trello por control de navegador no corre desatendido). Plan: inmediato (etiquetar correos procesados, remates semanal, idioma fijo), corto plazo (Trello por API, IDs deterministas), estructural (registro de causas y gestiones con marca de facturación). |
| iProyecta | Migración de correo a Google Workspace (≈78 GB, 12 casillas) | Informe N°1 del 02/08/2026. Cambio de MX planificado sáb 8 ago; configuración de equipos lun 10; cierre fines de agosto. Lección: verificar conteos reales, la 1ª pasada reportó "completado" con solo 1.272 correos. |
| Agrovial | Cotización enviada (jul 2026) | Hacer seguimiento. |
| Tecnigen | Propuesta | Pendiente de preparar. |
| ISO Consulting | Postulación / alianza | Pendiente enviar currículum. |
| Raúl | Sitio web | Publicado. |
| Mutual | Prospección | Ya se preguntó por proyectos; dar seguimiento. |
| Radio Ritmo (San Vicente) | Difusión: entrevistas y sesiones de ciberseguridad | Recurrente. |

## 6. Cómo comunica iData (tono y analogías)

- Lenguaje simple, cercano, sin tecnicismos ni alarmismo: "Sin susto. Con soluciones reales."
- Público: almacenero, contador, dueño de pyme, funcionario municipal, profesional independiente.
- Analogías de la casa:
  - **Antivirus** = guardia con una lista de fotos de ladrones conocidos.
  - **EDR** = cámara inteligente que detecta comportamientos sospechosos y actúa.
  - **Contraseñas en el navegador** = cuaderno amarillo con todas las llaves sobre el mesón.
  - **Trabajo remoto sin política** = sucursal sin cerradura.
  - **VPN ausente** = enviar la información escrita en una postal.
- Gestores de contraseñas recomendados: KeePass (gratis), Bitwarden (gratis/bajo costo), 1Password (pago).
- 5 reglas de trabajo remoto: equipo separado, VPN obligatoria, bloqueo de pantalla a los 2 min,
  MFA en todo, reportar de inmediato.
- 3 acciones gratuitas para hoy: dejar de guardar claves en Chrome (KeePass), activar doble factor
  en el correo, conversar hoy las reglas básicas con quien trabaja desde casa.
- Frase de cierre: "En ciberseguridad, lo que no saben SÍ puede hacerles daño. Actúen antes."

## 7. Principios de trabajo de iData

- **"El motor sincroniza, el modelo interpreta, el humano valida."** Lo verificable de forma exacta
  va a código determinista; lo que requiere juicio va al modelo; nada sale a un cliente sin
  aprobación humana explícita.
- Todo trabajo debe dejar rastro reutilizable: estado persistente, identificadores únicos,
  entregables almacenados.
- Confidencialidad: NDA siempre; no mezclar contexto de distintos clientes.
- Verificar resultados reales, no confiar en el "completado" de una herramienta.
- Formato de documentos: encabezado "IDATA CHILE SPA · RUT · dirección · correo", resumen
  ejecutivo primero, tablas para datos, próximos pasos numerados, firma "M. Rivas".
