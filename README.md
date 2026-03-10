#Ecommerce Api Gateway

## Giới thiệu

Điều hướng đến các dịch vụ logic cho dự án
### Thông tin chung của dự án
[Thông tin chung dự án](https://github.com/nguyenthinh28902/mini-project-ecommerce).


## 🛠 Công nghệ sử dụng
- **Framework:** .NET Core Api, **YARP**.
- **Database:** SQL Server (Entity Framework Core)
- **Giao thức:** OpenID Connect (OIDC) & OAuth
- **Khác:** Redis cache (chạy bằng Docker),
---

## 🔄 Workflow (Luồng xác thực)
### Cấu hình xác thực tại Web
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-cms-web).

### Xác thực tại identity
[Xem tiếp](https://github.com/nguyenthinh28902/ecommerce-identity-server-cms).

### Xác thực tại Getaway 
- Thực hiện xác thực jwt client.[(JwtAuthenticationExtensions.cs](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms/blob/main/Ecommerce.ApiGateway.Cms/Common/Auth/JwtAuthenticationExtensions.cs)
+ Kiểm tra tính hợp lệ của token
```csharp
      services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // IdentityServer URL
                    options.Authority = internalAuth.Issuer;
                    options.RequireHttpsMetadata = false; // Dev mode

                    // BẮT BUỘC: Lưu token để dùng trong AddTransforms (Token Relay)
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = internalAuth.Issuer,
                        ValidateAudience = false, // gateway không kiểm tra audience
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(20),// Khớp thời gian chính xác giữa Gateway và IdentityServer
                    };
                });
```
+ Kiểm tra quyền:
```csharp
services.AddAuthorization(options =>
            {
                options.AddPolicy("UserReadPolicy", policy =>
                    policy.RequireClaim("scope", "user.read", "user.internal"));
                options.AddPolicy("UserWritePolicy", policy =>
                    policy.RequireClaim("scope", "user.write", "user.internal"));
                options.AddPolicy("InternalPolicy", policy =>
                    policy.RequireClaim("scope", "user.internal"));
                options.AddPolicy("ProductPolicy", policy =>
                    policy.RequireClaim("scope", "product.read", "product.write", "product.internal"));
                options.AddPolicy("OrderPolicy", policy =>
                    policy.RequireClaim("scope", "order.read", "order.write", "order.internal"));
            });
```
- Thêm các thông tin user cho request và thực hiện đổi token hệ thống để đi tiếp đến các dịch vụ nghiệp vụ. [GatewayExtensions](https://github.com/nguyenthinh28902/ecommerce-api-gateway-cms/blob/main/Ecommerce.ApiGateway.Cms/Common/Auth/GatewayExtensions.cs)
+ Gateway dùng client identity riêng để xin token mới.
+ Token lưu cache cực ngắn, trách xin token nhiều lần.
Truyền dữ liệu user được lưu trong cache
```csharp
 transformContext.ProxyRequest.Headers.Add("X-User-Id", sub);

                        if (!string.IsNullOrEmpty(userInfo?.Email))
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-Email", userInfo.Email);
                        }
                        if (userInfo?.Roles != null && userInfo.Roles.Any())
                        {
                            // Chuyển List<string> thành "Admin,Manager,Editor"
                            var rolesString = string.Join(",", userInfo.Roles);
                            var scopesString = string.Join(",", userInfo.Scopes);

                            // Sử dụng TryAddWithoutValidation để tránh lỗi format header
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Roles", rolesString);
                            transformContext.ProxyRequest.Headers.TryAddWithoutValidation("X-User-Scopes", scopesString);

                        } 
                        if (userInfo?.WorkplaceId != null)
                        {
                            transformContext.ProxyRequest.Headers.Add("X-User-WorkplaceId", userInfo.WorkplaceId.ToString());
                            transformContext.ProxyRequest.Headers.Add("X-User-WorkplaceType", userInfo.WorkplaceType);
                        }
```
Đổi token hệ thống để đi tiếp
```csharp
// --- TOKEN MỚI (SERVICE-TO-SERVICE) ---
         systemToken = await tokenService.GetSystemTokenAsync();
          logger.LogInformation("New System Token (Service-to-Service): Bearer {Token}", systemToken);
          // Ghi đè hoặc thêm Token hệ thống vào Header Authorization
          transformContext.ProxyRequest.Headers.Authorization =
          new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", systemToken);
```

### Xác thực tại Service (Product servcie)
[Xem tiếp](https://github.com/nguyenthinh28902/Ecom.ProductService).
