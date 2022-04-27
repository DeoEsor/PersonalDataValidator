# Personal Data Validator Project
Данный проект разрабатывается как лабораторная работа по курсу "Разработка прикладных компьютерных систем"
## Стек технологий
![.Net](https://img.shields.io/badge/.NET-000?style=for-the-badge&logo=.net&logoColor=informational)
![C#](https://img.shields.io/badge/c%23-000.svg?style=for-the-badge&logo=c-sharp&logoColor=blue)
![WPF](https://img.shields.io/badge/WPF-00000F?style=for-the-badge&logo=c-sharp&logoColor=blue)
![gRPC](https://img.shields.io/badge/gRPC-00000F?style=for-the-badge&logoColor=blue)
![MSSql](https://img.shields.io/badge/MSSQL-00000F?style=for-the-badge&logo=Microsoft-SQL-Server&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-000?style=for-the-badge&logo=redis&logoColor=red)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-000?style=for-the-badge&logo=RabbitMQ&logoColor=blue)
![Unity](https://img.shields.io/badge/Unity_(DI)-000?style=for-the-badge&logoColor=blue)
![Docker](https://img.shields.io/badge/Docker-000?style=for-the-badge&logo=Docker&logoColor=blue)
![K8S](https://img.shields.io/badge/Kubernetes-000?style=for-the-badge&logo=Kubernetes&logoColor=blue)
## Архитектура проекта
Архитектура проекта представляет собой 3-ех звенчатую систему :
* Звено 1 - Клиентские инстансы
    * Заполнение данных
    * Визуализация данных
    * gRPC клиент
* Звено 2 - Серверные инстансы
  ![K8S](https://img.shields.io/badge/Kubernetes-ccc?style=for-the-badge&logo=Kubernetes&logoColor=blue)
  ![Docker](https://img.shields.io/badge/Docker-ccc?style=for-the-badge&logo=Docker&logoColor=blue)
    * Медиатор - серверное приложение, что отвечает за делегацию валидации данных конкретным инстансам
      ![RabbitMQ](https://img.shields.io/badge/RabbitMQ-000?style=for-the-badge&logo=RabbitMQ&logoColor=blue)
    * Стек валидаторов - комплекс приложений, которые выполняют валидацию конкретного типа данных
<!-- * Звено 3  - БД ![MySql](https://img.shields.io/badge/MySQL-00000F?style=for-the-badge&logo=mysql&logoColor=white)
    * БД для хранения партий
    * БД для хранения данных пользователей-->