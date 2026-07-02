```mermaid
classDiagram

%% ======================
%% ENTIDADES
%% ======================

class BaseEntity{
    +Guid Id
    +DateTime CreatedDateTime
    +DateTime? UpdatedDateTime
    +bool IsDeleted
}

class User{
    +string FirstName
    +string LastName
    +string Email
    +string Password
    +int RoleId
    +bool IsActive
}

class UserRole{
    +int Id
    +UserRoleEnum Name
    +string Description
}

class Shipment{
    +string Origin
    +string Destination
    +decimal Price
    +int ShipmentTypeId
    +int ShipmentStatusId
    +Guid CreatedByUserId
    +int CreatedByRoleId
    +Guid? OnBehalfOfClientId
}

class ShipmentType{
    +int Id
    +ShipmentTypeEnum Name
    +decimal Price
    +string Description
}

class ShipmentStatus{
    +int Id
    +ShipmentStatusEnum Name
    +string Description
}

class Inquiry{
    +string FirstName
    +string LastName
    +string Email
    +string Message
    +string? Response
    +string? ClientReply
    +InquiryStatusEnum Status
    +Guid CreatedByUserId
    +Guid? RespondedByUserId
}

%% ======================
%% HERENCIA
%% ======================

BaseEntity <|-- User
BaseEntity <|-- Shipment
BaseEntity <|-- Inquiry

%% ======================
%% RELACIONES
%% ======================

UserRole "1" --> "0..*" User : Role

User "1" --> "0..*" Shipment : creates

User "0..1" --> "0..*" Shipment : on behalf of

UserRole "1" --> "0..*" Shipment : created by role

ShipmentType "1" --> "0..*" Shipment

ShipmentStatus "1" --> "0..*" Shipment

User "1" --> "0..*" Inquiry : creates

User "0..1" --> "0..*" Inquiry : responds
