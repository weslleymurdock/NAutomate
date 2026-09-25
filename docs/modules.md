# Modules

A module is precompiled C# code selected by a stable module ID. Workflow JSON contains only the module ID and serializable parameters.

## Contract boundary

Module runtime contracts belong in `NAutomate.Abstractions`. Implementations may depend on external automation libraries, but Core remains independent of those libraries.

Modules can expose automation services through annotated interfaces. Service and operation metadata is part of the module contract used by future Web/MAUI designers.

## Registry

Core owns the runtime registry. The registry receives already-composed module instances from the host; it does not discover or instantiate official modules.

Workflow module IDs are case-insensitive and duplicate registration is rejected.

## Appium module

`NAutomate.Modules.Appium` is the first stateful automation module.

It exposes:

- `IAndroidAppiumService`
- `IIOSAppiumService`

Both services expose annotated operations backed by the Appium .NET client. Workflow parameters are bound by name to the selected service operation's method parameters.

The Appium service owns the active driver/session state. A workflow never serializes an Appium driver, element, socket, or other runtime resource.

Element operations return opaque workflow resource IDs such as `element-1`. Later operations can consume those IDs without putting the actual `AppiumElement` into workflow JSON.

Appium session creation is performed by the service's `start-session` operation. The host does not need to construct a driver before a workflow supplies the endpoint and capabilities.

The current implementation targets Appium.WebDriver 9.0.0. The package is an extension of the Selenium .NET binding and supports .NET 10. Future work will add Docker/Appium server orchestration and live device rendering without moving that process infrastructure into Core.

## UI metadata

The following attributes are available to module UI designers:

- `AutomationServiceAttribute`
- `AutomationOperationAttribute`
- `AutomationParameterAttribute`

They provide stable IDs, display names and descriptions without coupling Core to a UI framework.


## Selenium

The official Selenium module is provided by `NAutomate.Modules.Selenium` and uses Selenium.WebDriver 4.49.0. It exposes one stateful service per browser:

- `selenium.chrome`
- `selenium.firefox`
- `selenium.edge`

Each service supports session lifecycle, navigation, element lookup and interaction, page metadata, and screenshots. Selenium Manager is used by the local WebDriver constructors, so a workflow does not need to install driver executables manually.

Selenium element references are opaque runtime identifiers. They are held by the service instance and are not persisted into workflow JSON.

## Execution environments

Workflows can declare global and step-local variables. Saved execution environments contain the same variable keys for a given automation, while newly introduced variables are synchronized into every saved environment with an empty value.

Workflow parameters can reference values using:

- `${name}` for global variables.
- `${stepId:name}` for local variables.

Environment values are persisted separately from workflow JSON under the host application-data directory. The workflow remains declarative; environments only provide runtime values.
