# Task Orchestrator API Gateway

## Описание
Сервис осуществляет управление задачами, позволяет создавать задачи, инициировать их выполнение и отслеживать прогресс выполнения задач в реальном времени.
Система состоит из двух сервисов:
- API Gateway:
    - Принимает HTTP-запросы от фронтенда, являясь центральной точкой входа в систему
    - Управляет WebSocket-соединениями через SignalR
    - Публикует задачи в Kafka
    - Является gRPC-клиентом для Workers Service
- [Workers Service](https://github.com/KanatnikovMax/Task-Orchestrator-Workers-Service):
    - Потребляет задачи из Kafka
    - Отправляет прогресс выполнения через SignalR
    - Сохраняет метаданные задач в базу данных
    - Содержит серверный gRPC-метод для инициирования выполнения задачи
___
## Используемые технологии
Для реализации использовались следующие технологии:
- Microsoft.NET Core
- Entity Framework Core
- PostgreSQL
- SignalR
- Apache Kafka
- gRPC
- Redis
- Docker
- Nginx