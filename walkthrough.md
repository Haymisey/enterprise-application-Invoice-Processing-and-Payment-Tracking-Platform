# Enterprise Invoice Processing Platform - Implementation Walkthrough

This document outlines the architectural improvements and features implemented to ensure a robust, scalable, and resilient platform.

## 1. Architectural Improvements

### Centralized Exception Handling
Implemented a `GlobalExceptionHandler` using the `IExceptionHandler` interface (new in .NET 8). This provides a consistent RFC 7807 problem details response for all API errors.

### MediatR Validation Pipeline
Integrated `FluentValidation` with a MediatR `IPipelineBehavior`. All incoming requests are automatically validated before reaching the handlers, preventing invalid data from entering the domain.

### Transactional Outbox Pattern
To ensure eventual consistency across modules without distributed transactions, we implemented the Outbox pattern:
- **Interceptors**: `ConvertDomainEventsToOutboxMessagesInterceptor` automatically saves domain events to the module's `OutboxMessages` table within the same transaction.
- **Background Worker**: `OutboxProcessor` periodically polls all module databases and publishes pending messages to RabbitMQ.

## 2. Messaging & Integration
Implemented a RabbitMQ `EventBus` for resilient asynchronous communication between modules. This decouples the `InvoiceManagement` module (which produces events) from the `PaymentTracking` module (which consumes them to schedule payments).

## 3. Verification Steps

### Automated Testing
Use the provided `test-api.ps1` script to verify the end-to-end flow:
1. Register a Vendor.
2. Create an Invoice for that Vendor.
3. Validate that the system correctly rejects invalid requests (e.g., negative amounts).

### Database Schema
Verified that all modules now have a dedicated `OutboxMessages` table in their respective schemas (`vendor`, `invoice`, `payment`).

---
*Developed with best practices in Clean Architecture and Microservices patterns.*
