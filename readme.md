**Projede Identity.Dapper ve NLog kütüphaneleri kullanarak log tabanlı kullanıcı yönetimi sistemi oluşturdum.**

# Identity.Dapper

[![Build Status](https://travis-ci.org/grandchamp/Identity.Dapper.svg?branch=master)](https://travis-ci.org/grandchamp/Identity.Dapper)
[![NuGet](https://img.shields.io/nuget/v/Identity.Dapper.svg?style=flat)](https://www.nuget.org/packages/Identity.Dapper/)

Nuget package (Eg: Identity.Dapper.SqlServer).

Veritabanı yapılandırılması **DapperIdentity** **DapperIdentityCryptography** 
```JSON
"DapperIdentity": {
    "ConnectionString": "Connection string of your database",
    "Username": "user",
    "Password": "123"
},
"DapperIdentityCryptography": {
    "Key": "Base64 32 bytes key",
    "IV": "Base64 16 bytes key"
}
```

**Admin Girişi:**  
UserName: "admin"
Password: "123"

**Yazar Girişi:**  
UserName: "yazar"
Password: "123"

**Üye Girişi:**  
UserName: "üye"
Password: "123"
