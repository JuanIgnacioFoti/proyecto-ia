# Agentes Disponibles

Este proyecto usa agentes especializados para automatizar diferentes áreas del desarrollo.

## 🏗️ Agentes de Implementación

### @backend-implement
**Propósito**: Implementa tareas backend siguiendo Clean Architecture y SOLID.

**Capacidades**:
- Controllers, Services, Repositories
- Validaciones de negocio
- Manejo de excepciones
- Handoff automático a `solid-reviewer` y `test-generator`

**Uso**:
```
@workspace /backend-implement T-2.3
```

---

### @frontend-implement  
**Propósito**: Implementa componentes y servicios Angular.

**Capacidades**:
- Componentes standalone
- Services con HttpClient
- Routing y guards
- Formularios reactivos

**Uso**:
```
@workspace /frontend-implement T-3.2
```

---

## 🎨 Agente de Diseño

### @ux-designer
**Propósito**: Diseña y mejora interfaces de usuario siguiendo Material Design y accesibilidad.

**Capacidades**:
- Layouts responsive (mobile/tablet/desktop)
- Componentes con Tailwind CSS
- Accesibilidad WCAG 2.1 AA
- Estados: loading, error, empty
- Micro-interacciones

**Uso**:
```
@workspace /ux-designer mejora la página de registro
@workspace /ux-designer diseña el dashboard del organizador
```

---

## 🧪 Agente de Testing

### @qa-tester
**Propósito**: Genera tests unitarios, integración y E2E con análisis de coverage.

**Capacidades**:
- Unit tests: MSTest + Moq (C#), Jasmine/Karma (Angular)
- Integration tests: WebApplicationFactory
- E2E tests: Playwright
- Coverage analysis con reportes
- Test data generation

**Uso**:
```
@workspace /qa-tester genera tests para TournamentService
@workspace /qa-tester crea E2E test para registro de usuario
@workspace /qa-tester analiza coverage del backend
```

---

## 🚀 Agente de CI/CD

### @cicd-deployer
**Propósito**: Configura pipelines de CI/CD para deploy automático.

**Capacidades**:
- GitHub Actions workflows
- Docker multi-stage builds
- Deploy a Azure/AWS/DigitalOcean
- Database migrations automáticas
- Health checks y monitoring
- Secrets management

**Uso**:
```
@workspace /cicd-deployer configura GitHub Actions
@workspace /cicd-deployer optimiza Dockerfile  
@workspace /cicd-deployer setup Azure deployment
```

---

## 🔍 Agentes de Revisión

### @solid-reviewer
**Propósito**: Revisa código C# y valida cumplimiento de SOLID.

**Capacidades**:
- Analiza Single Responsibility, Open/Closed, etc.
- Scoring 0-10 con recomendaciones
- Identifica code smells

**Uso**: Automático (handoff desde `backend-implement`)

---

### @test-generator
**Propósito**: Genera unit tests MSTest con Moq para servicios C#.

**Capacidades**:
- AAA pattern (Arrange-Act-Assert)
- Mocking con Moq
- Edge cases y validaciones

**Uso**: Automático (handoff desde `backend-implement`)

---

## 🎯 Agente Orquestador

### @orchestrator
**Propósito**: Ejecuta todas las tareas secuencialmente con validación automática.

**Capacidades**:
- Lee tasks.md
- Ejecuta tareas en orden de dependencias
- Valida cada tarea antes de continuar
- Reporta progreso con tabla de estado

**Uso**:
```
@workspace /orchestrator ejecuta todas las tareas
```

---

## 📋 Agentes Speckit

Estos agentes son parte del sistema Speckit para gestión de especificaciones:

- `@speckit.specify` - Crea/actualiza feature specs
- `@speckit.plan` - Genera plan de implementación
- `@speckit.tasks` - Genera lista de tareas ordenadas
- `@speckit.implement` - Ejecuta plan de implementación
- `@speckit.analyze` - Analiza consistencia de artifacts
- `@speckit.clarify` - Identifica áreas subspecificadas
- `@speckit.checklist` - Genera checklist custom
- `@speckit.constitution` - Gestiona constitución del proyecto

### Git Helpers
- `@speckit.git.initialize` - Inicializa repo
- `@speckit.git.feature` - Crea feature branch
- `@speckit.git.commit` - Auto-commit después de comandos
- `@speckit.git.validate` - Valida convenciones de branch
- `@speckit.git.remote` - Detecta remote URL para GitHub

---

## 🔄 Workflow Típico

### Implementar Feature Completa
1. **Planificación**: `@speckit.specify` + `@speckit.plan` + `@speckit.tasks`
2. **Backend**: `@backend-implement` → auto-valida con `@solid-reviewer` y `@test-generator`
3. **Frontend**: `@frontend-implement` + `@ux-designer` para UI
4. **Testing**: `@qa-tester` para E2E y coverage
5. **Deploy**: `@cicd-deployer` para CI/CD setup

### Mejorar UI Existente
1. `@ux-designer` analiza y propone mejoras
2. Implementa cambios en templates
3. `@qa-tester` genera E2E tests para validar

### Setup Deployment
1. `@cicd-deployer` genera workflows de GitHub Actions
2. Crea Dockerfiles optimizados
3. Configura Azure/AWS
4. `@qa-tester` valida pipeline con tests

---

## 📚 Recursos

- **Agentes**: `.github/agents/*.agent.md`
- **Tasks**: `specs/001-esports-tournament-platform/tasks.md`
- **Plan**: `specs/001-esports-tournament-platform/plan.md`
- **Constitution**: `.specify/memory/constitution.md`
