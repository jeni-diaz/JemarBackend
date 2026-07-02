```mermaid
classDiagram

%% ======================
%% ENTIDADES
%% ======================

class User{
    +Guid Id
    +DateTime CreatedDateTime
    +DateTime? UpdatedDateTime
    +bool IsDeleted
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
    +Guid Id
    +DateTime CreatedDateTime
    +DateTime? UpdatedDateTime
    +bool IsDeleted
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
    +Guid Id
    +DateTime CreatedDateTime
    +DateTime? UpdatedDateTime
    +bool IsDeleted
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
%% ENUMS
%% ======================

class UserRoleEnum{
    Client
    Employee
    SuperAdmin
}

class ShipmentTypeEnum{
    Standard
    Express
}

class ShipmentStatusEnum{
    Pending
    InTransit
    Delivered
    Cancelled
}

class InquiryStatusEnum{
    New
    InProgress
    Answered
    Closed
}

%% ======================
%% RELACIONES
%% ======================

UserRole "1" <-- "0..*" User : Role

User "1" --> "0..*" Shipment : CreatedShipments
User "1" --> "0..*" Shipment : OnBehalfShipments

ShipmentType "1" <-- "0..*" Shipment
ShipmentStatus "1" <-- "0..*" Shipment
UserRole "1" <-- "0..*" Shipment : CreatedByRole

User "1" --> "0..*" Inquiry : CreatedInquiries
User "0..1" --> "0..*" Inquiry : RespondedInquiries

UserRole --> UserRoleEnum
ShipmentType --> ShipmentTypeEnum
ShipmentStatus --> ShipmentStatusEnum
Inquiry --> InquiryStatusEnum
