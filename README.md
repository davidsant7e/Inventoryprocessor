# Inventory Processor

Herramienta de escritorio en Windows Forms para cargar, procesar y exportar inventarios JSON.

## Requisitos

- .NET 6 SDK o superior  
- Windows 10 / 11

## Ejecución

```bash
git clone <url-del-repo>
cd InventoryProcessor
dotnet run
```

El archivo `input.json` de ejemplo está incluido en el repositorio.

## Uso

1. **Cargar JSON** — Abre un archivo de inventario `.json`.  
2. Editar filas en la grilla si se requiere (columnas Categoría, Status, SKU son editables).  
3. **Procesar** — Aplica las tres reglas de negocio y muestra el log de cambios.  
4. **Exportar JSON** — Disponible solo si el procesamiento fue exitoso. Guarda el archivo corregido.

## Decisiones de diseño

### Separación de capas

| Capa | Clase | Responsabilidad |
|------|-------|-----------------|
| Modelos | `Product`, `InventoryFile`, `ProcessResult` | Entidades del dominio y DTOs. Sin comportamiento ni dependencias. |
| Constantes de dominio | `DomainConstants` | Todas las cadenas y reglas del dominio: categorías, prefijos, keywords, status, mensajes de log y validación, textos de UI. |
| I/O | `FileService` / `IFileService` | Lee y escribe JSON con `System.Text.Json`. Normaliza ceros iniciales en números antes de deserializar. |
| Lógica | `InventoryProcessorService` / `IInventoryProcessor` | Aplica las reglas de negocio, valida, genera resumen. Sin referencias a controles ni archivos. |
| UI | `MainForm` | Presenta datos, captura eventos, delega a las capas inferiores. Sin reglas de negocio. |
| Constantes de UI | `UIColors` | Todos los colores de la interfaz agrupados por sección. Sin dependencia de dominio. |

### Interfaces como contratos

Se definieron `IFileService` e `IInventoryProcessor` para desacoplar las capas. La composición se hace manualmente en `Program.cs`, sin framework de DI, apropiado para el alcance de la prueba.

### Composición manual en Program.cs

```csharp
var fileService = new FileService();
var processor   = new InventoryProcessorService();
Application.Run(new MainForm(fileService, processor));
```

### Reglas de negocio

- **Categorización**: tabla de palabras clave insensible a mayúsculas; fallback a `"Sin categoría"` con advertencia en log.  
- **Status**: recalculado siempre desde cero (stock vs minStock), no se confía en el valor original del JSON.  
- **SKU**: se conserva el SKU del producto con id menor en caso de duplicado; los números nuevos se asignan incrementando desde el último SKU existente en la categoría.  
- **Precio**: productos con `unitPrice <= 0` se reportan durante el procesamiento y bloquean la exportación.

### Bonus implementados

- Coloreado de filas según status (rojo = critical/out_of_stock, naranja = reorder).  
- Panel de resumen tras procesar: total, modificados, críticos, reorder, ok.  
- Columnas Categoría y Status con selector desplegable (valores válidos del dominio).  
- Tolerancia a ceros iniciales en valores numéricos del JSON (ej: `0350000` → `350000`).

## Archivos de constantes

### `Models/DomainConstants.cs`

Clase estática única para todas las cadenas del dominio y de la interfaz. Elimina strings literales dispersos en el código. Se importa con `using static InventoryProcessor.Models.DomainConstants`.

| Clase | Contenido |
|-------|-----------|
| `Categories` | Nombres de las 5 categorías: Electrónica, Periféricos, Mobiliario, Accesorios, Sin categoría |
| `Prefixes` | Prefijos de SKU por categoría: `ELC`, `PER`, `MOB`, `ACC`, `SIN` |
| `Keywords` | Arrays de palabras clave para la categorización automática por nombre de producto |
| `Status` | Valores de status: `ok`, `critical`, `reorder`, `out_of_stock` |
| `AppUI` | Textos de la interfaz: título, botones, etiquetas, columnas de la grilla, diálogos, prefijos del log |
| `ProcessMessages.Log` | Formatos de los mensajes que aparecen en el log durante el procesamiento |
| `ProcessMessages.Errors` | Formatos de los mensajes de error de validación |

#### Prefijos de SKU

| Categoría | Prefijo | Ejemplo |
|-----------|---------|---------|
| Electrónica | `ELC` | `ELC-0003` |
| Periféricos | `PER` | `PER-0001` |
| Mobiliario | `MOB` | `MOB-0002` |
| Accesorios | `ACC` | `ACC-0004` |
| Sin categoría | `SIN` | `SIN-0001` |

### `UI/UIColors.cs`

Clase estática interna con todos los colores de la interfaz, organizados por sección de la ventana. Ninguna clase fuera de `UI/` tiene acceso a ella.

| Clase | Colores definidos |
|-------|-------------------|
| `Toolbar` | Fondo, color de cada botón (Load, Process, Export), color del texto |
| `InfoPanel` | Fondo, color del nombre del almacén, color de la fecha |
| `SummaryPanel` | Fondo, color por tipo de contador (Critical, Reorder, Ok, Default) |
| `Grid` | Fondo, líneas de separación, color de fila por status |
| `Log` | Fondo oscuro, colores de texto por tipo de mensaje (éxito, error, advertencia, evento, encabezado) |
