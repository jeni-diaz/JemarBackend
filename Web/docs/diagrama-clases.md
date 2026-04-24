# Diagrama de Clases

```mermaid
classDiagram

class User
class Client
class Employee
class SuperAdmin
class Shipment
class ShipmentStatus
class ShipmentType
class Inquiry
class ShipmentStatusEnum
class ShipmentTypeEnum

User <|-- Client
User <|-- Employee
User <|-- SuperAdmin

Client "1" --> "0..*" Shipment
Employee "1" --> "0..*" Shipment
Employee "1" --> "0..*" Inquiry

Shipment "1" --> "1" ShipmentStatus
Shipment "1" --> "1" ShipmentType

ShipmentStatus "1" --> "1" ShipmentStatusEnum
ShipmentType "1" --> "1" ShipmentTypeEnum
