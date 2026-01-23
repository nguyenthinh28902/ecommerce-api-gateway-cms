# EcommerceApiGatewayCMS – API Gateway

## Giới thiệu

`EcommerceApiGatewayCMS` là **API Gateway** của hệ thống Ecommerce CMS, được xây dựng trên **ASP.NET Core** và **YARP (Yet Another Reverse Proxy)**.

Dự án đóng vai trò là **điểm truy cập duy nhất (Single Entry Point)** cho các client (CMS, Admin, Frontend), đồng thời tích hợp **Identity Service** để thực hiện xác thực và **cấp phát JWT Token cho client**.

API Gateway chịu trách nhiệm:

* Tiếp nhận và định tuyến request từ client đến các backend services
* Kết nối với **Identity Service** để xác thực người dùng
* Trả về **JWT Access Token** cho client sau khi đăng nhập thành công
* Đảm bảo kiểm soát truy cập và bảo mật ở tầng gateway

> Hiện tại README chỉ mô tả **mục đích và vai trò của dự án**, chưa bao gồm hướng dẫn build, cấu hình hay triển khai.
