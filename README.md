# Enterprise API Gateway - Cửa ngõ điều hướng & Bảo mật tập trung

## 📝 Giới thiệu
Hệ thống API Gateway được xây dựng trên nền tảng **YARP (Yet Another Reverse Proxy)** của Microsoft. Đây là điểm tiếp nhận duy nhất cho mọi yêu cầu từ phía Client (CMS), chịu trách nhiệm điều hướng thông minh, kiểm soát truy cập và thực hiện chuyển đổi định danh (Token Exchange) trước khi gửi yêu cầu tới các dịch vụ nghiệp vụ.
### 🔗 Core Security & Implementation (Liên kết kỹ thuật trọng tâm)

> **Tổng quan dự án xem tại đây:** [Xem đầy đủ kiến trúc tại đây](https://github.com/nguyenthinh28902/mini-project-ecommerce)

Để đi sâu vào các cấu hình bảo mật hệ thống, bạn có thể tham khảo trực tiếp tại các module sau:

* **Client Security:** Triển khai OIDC Middleware, quản lý Secure Cookie và luồng Challenge.
  * [Cấu hình tại Web CMS](https://github.com/nguyenthinh28902/ecommerce-cms-web)
* **Identity Provider:** Định nghĩa Resource, Scope và Custom Profile Service để mapping Claims.
  * [Cấu hình tại Identity Server](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms)
* **API Gateway (YARP):** Quản lý Reverse Proxy Routing và thiết lập Auth Policy tập trung.
  * [Cấu hình tại Gateway CMS](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms)
* **Resource Server:** Cấu hình JWT Bearer và phân quyền dựa trên Policy (Policy-based Authorization).
  * [Cấu hình tại Product Service](https://github.com/nguyenthinh28902/Ecom.ProductService)

---
---

## 🛠 Công nghệ & Giải pháp hạ tầng
- **Core Engine:** .NET Core API & **YARP**.
- **Security:** JWT Bearer Authentication & Policy-based Authorization.
- **Performance:** Redis Cache (Docker) để lưu trữ thông tin định danh và tối ưu hóa việc cấp phát Token.
- **Data Access:** Entity Framework Core & SQL Server.

---

## 🔐 Technical Implementation (Triển khai kỹ thuật)

### 1. Centralized JWT Validation (Xác thực JWT tập trung)
Gateway đóng vai trò là chốt chặn đầu tiên để kiểm tra tính hợp lệ của Token từ Identity Server.

* **File:** [JwtAuthenticationExtensions.cs](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms/blob/main/Ecommerce.ApiGateway.Cms/Common/Auth/JwtAuthenticationExtensions.cs)
* **Giải pháp:** Cấu hình `SaveToken = true` để duy trì ngữ cảnh xác thực và thiết lập `ClockSkew` cực ngắn (20 giây) để đảm bảo sự đồng bộ thời gian tuyệt đối với Identity Server.

```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.Authority = internalAuth.Issuer;
        options.SaveToken = true; // Lưu token để thực hiện Token Relay/Exchange
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(20) // Khớp thời gian chính xác giữa các tầng
        };
    });
```

### 2. Policy-Based Authorization (Phân quyền dựa trên Policy)
Định nghĩa các chính sách truy cập nghiêm ngặt dựa trên `scope` của Token ngay tại cửa ngõ hệ thống.

* **Giải pháp:** Phân tách rõ ràng các quyền đọc/ghi (`user.read`, `user.write`) và các quyền truy cập nội bộ (`user.internal`) để bảo vệ tài nguyên Microservices.

```csharp
options.AddPolicy("UserReadPolicy", policy => 
    policy.RequireClaim("scope", "user.read", "user.internal"));
options.AddPolicy("ProductPolicy", policy => 
    policy.RequireClaim("scope", "product.read", "product.write", "product.internal"));
```

### 3. Identity Transformation & Token Exchange (Chuyển đổi định danh)
Đây là kỹ thuật quan trọng nhất giúp Gateway bảo mật thông tin người dùng và thực hiện giao tiếp Service-to-Service.

* **File:** [GatewayExtensions.cs](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms/blob/main/Ecommerce.ApiGateway.Cms/Common/Auth/GatewayExtensions.cs)
* **Giải pháp:** * **Header Transformation:** Gateway tự động trích xuất Claims (Roles, Email, WorkplaceId) từ cache và đính kèm vào Header `X-User-*`.
    * **Token Exchange:** Gateway sử dụng Client Credentials riêng để hoán đổi lấy một **System Token** mới, dùng để gọi các dịch vụ Backend.

```csharp
// Đính kèm ngữ cảnh người dùng vào Proxy Request
transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);
transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", rolesString);

// --- TOKEN EXCHANGE (SERVICE-TO-SERVICE) ---
// Gateway đổi lấy Token hệ thống để đi tiếp vào vùng nội bộ
var systemToken = await tokenService.GetSystemTokenAsync();
transformContext.ProxyRequest.Headers.Authorization = 
    new AuthenticationHeaderValue("Bearer", systemToken);
```
---
