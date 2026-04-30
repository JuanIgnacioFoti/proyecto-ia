# Plataforma de Campeonatos de Esports - Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)
- **Separación de responsabilidades estricta**: Los controladores solo manejan HTTP, los repositorios solo acceso a datos, la lógica de negocio vive en servicios
- **Flujo de dependencias unidireccional**: La lógica de negocio NUNCA depende de infraestructura (EF Core, ASP.NET, etc.)
- **Testabilidad por diseño**: Toda lógica de negocio debe ser testeable sin dependencias externas mediante inyección de dependencias

### II. SOLID Principles (Mínimo 2 Documentados)
- **Single Responsibility**: Cada clase tiene una única razón para cambiar
- **Open/Closed**: Sistema de puntuación DEBE ser extensible sin modificar código existente (Strategy Pattern obligatorio)
- **Liskov Substitution**: Los sistemas de puntuación deben ser intercambiables
- **Interface Segregation**: Interfaces específicas y cohesivas
- **Dependency Inversion**: Depender de abstracciones, no de implementaciones concretas
- **Documentación obligatoria**: Identificar y justificar al menos 2 principios aplicados con referencias a clases/métodos específicos

### III. Clean Code (Siempre)
- **Nombres en inglés**: Todas las clases, métodos, variables y propiedades en inglés
- **Nombres expresivos**: Los nombres deben revelar intención sin necesidad de comentarios
- **Funciones pequeñas**: Una sola responsabilidad por función/método
- **Sin código comentado**: Eliminar todo código muerto o comentado
- **Sin números mágicos**: Usar constantes con nombres descriptivos
- **Sin prints de debug**: Limpiar después de debugging
- **CERO Comentarios** (NON-NEGOTIABLE):
  - ❌ **NO comentar QUÉ hace el código**: El código debe ser auto-explicativo
  - ❌ **NO comentar POR QUÉ**: Si necesitas explicar el porqué, el código está mal estructurado
  - ❌ **NO comentarios obvios**: Eliminar todo comentario redundante
  - ❌ **NO documentación XML**: Innecesaria si los nombres son buenos
  - **Estrategia**:
    - ✅ **Nombres largos y descriptivos** para métodos/variables privados
    - ✅ **Nombres cortos pero claros** para métodos públicos importantes
    - ✅ **Dividir en múltiples funciones** si es muy largo o complejo
    - ✅ **Extract Method** si necesitas comentar una sección
  - **Regla de oro**: Si necesitas un comentario, refactoriza primero. El código es la documentación.

### IV. RESTful API Standards
- **Recursos como sustantivos en plural**: `/api/tournaments`, `/api/teams`, `/api/players`
- **Verbos HTTP correctos**: GET (lectura), POST (creación), PUT (actualización completa), PATCH (actualización parcial), DELETE (eliminación)
- **Códigos de estado apropiados**:
  - 200 OK, 201 Created, 204 No Content
  - 400 Bad Request (validación), 401 Unauthorized, 403 Forbidden, 404 Not Found
  - 500 Internal Server Error (sin exponer detalles internos)
- **Estructura consistente de respuestas**: JSON con convenciones camelCase en frontend, PascalCase en backend

### V. Testing (NON-NEGOTIABLE para features con *)
- **Unit Tests obligatorios**: MSTest + Moq para:
  - Alta de torneo
  - Registro de resultados
  - Registro de cuenta de jugador
  - Inscripción a torneos
- **Nombres descriptivos**: Los tests deben explicar el escenario sin mirar el código
- **Test Cases manuales documentados**: Precondiciones, pasos, resultado esperado y obtenido
- **Cobertura con criterio**: Happy paths + casos borde + escenarios de error

## Stack Tecnológico (Inmutable)

### Backend
- **.NET 10** con **EF Core** (Code-First)
- **SQL Server** como base de datos
- **MSTest + Moq** para testing
- **Autenticación**: Implementación a criterio del equipo (JWT, Session, etc.)

### Frontend
- **Angular 21**
- **TypeScript** estricto
- Integración con backend vía API REST

### Tooling
- **GitHub** para control de versiones
- **GitHub Issues** para gestión de features (cada issue vinculado a su PR)
- **GitHub Copilot** como asistente de IA principal (Student Pack)

## Manejo de Errores Centralizado (NON-NEGOTIABLE)

### Exception Handling Middleware
- **Validación (400)**: Errores de validación con mensajes descriptivos en español
- **Negocio (400/404/409)**: Errores de lógica de negocio con códigos apropiados
- **Autenticación (401)**: Requests no autenticados
- **Autorización (403)**: Requests sin rol requerido
- **Server (500)**: Excepciones no controladas SIN exponer detalles internos

### Estructura de respuesta de error
```json
{
  "error": "Mensaje descriptivo del error",
  "details": ["Lista de validaciones fallidas"] // opcional
}
```

## Procesos de IA

### Archivo de Contexto (.github/copilot-instructions.md)
**Contenido permitido** (solo info que el agente NO puede inferir del código):
- Restricciones no estándar del proyecto
- Convenciones que rompen el estándar de .NET/Angular
- Landmines: código que no debe modificarse o tiene comportamientos no obvios
- **Evolución documentada**: Trackear cómo muta el archivo durante el desarrollo

**Prohibido incluir**:
- Estructura de carpetas (el agente la lee)
- Stack tecnológico (ya está en código)
- Convenciones estándar de C# o Angular

### Agent Skill Propia (Obligatorio)
- **Mínimo 1 skill custom** (revisora, generadora, validadora, etc.)
- **3 casos de prueba documentados** antes de considerarla terminada:
  1. Prompt de prueba
  2. Criterio de éxito concreto (qué debe producir, pasos, convenciones)
  3. Resultado obtenido (si se ejecutó) o justificación del criterio
  4. Ajustes realizados o propuestos

### Diario de Prompts (Obligatorio)
Documentar ejemplos de uso de IA en estas 4 etapas del SDLC:
1. **Requerimientos**: Analizar, clarificar o refinar requerimientos
2. **Implementación y Debugging**: Escritura o corrección de código
3. **Unit Tests**: Generación o mejora de tests (¿cubrió casos borde?)
4. **Test Cases Manuales**: Generación de casos de prueba (¿fueron suficientes?)

**Estructura por prompt**:
- Contexto: ¿Qué problema resolvían?
- Técnica y justificación: Zero-Shot, Few-Shot, CoT, Role, etc. ¿Por qué esa técnica?
- El prompt exacto (con iteraciones si hubo)
- Análisis del output: ¿Qué funcionó? ¿Qué requirió revisión?
- Decisión y aprendizaje: ¿Aceptaron, modificaron o descartaron? ¿Por qué?

### Gestión de Tokens
- El plan gratuito tiene límites: **responsabilidad del equipo**
- Si se agotan tokens: continuar manualmente (no es causal de extensión)

## Reglas de Negocio Críticas

### Estados de Torneo (Unidireccionales)
- **Flujo**: Borrador → Abierto → En Curso → Finalizado
- **Transiciones**: Solo avanzar, nunca retroceder
- **Pueden saltar estados** pero nunca ir hacia atrás
- **Borrador**: Solo visible para el organizador
- **Abierto**: Equipos pueden inscribirse
- **En Curso**: No se aceptan nuevas inscripciones
- **Sistema de puntuación**: No modificable luego de comenzado

### Sistema de Puntuación (Strategy Pattern Obligatorio)
- **Estándar**: 3 victoria, 1 empate, 0 derrota
- **WTA (Winner Takes All)**: 3 victoria, 0 empate/derrota
- **Personalizada**: Valores definidos por organizador
- **Extensibilidad**: Agregar nuevos sistemas SIN modificar código existente

### Roles y Permisos
- **Organizador**: Gestiona solo SUS propios torneos
- **Capitán**: Gestiona solo SU equipo, inscribe en torneos
- **Jugador**: Un solo equipo activo por videojuego, puede ser capitán de uno por juego

## Governance


### Compliance
- Todo PR debe verificar cumplimiento de esta constitución
- Complejidad debe ser justificada
- Esta constitución supersede cualquier otra práctica
- Amendments requieren documentación y justificación

**Version**: 1.0.0 | **Ratified**: 2026-04-30 | **Last Amended**: 2026-04-30
