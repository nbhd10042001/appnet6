## Package cài đặt MSSQL và MySQL
dotnet add package MySql.Data
dotnet add package MySql.Data.EntityFramework
dotnet add package System.Data.SqlClient
dotnet add package Newtonsoft.Json

## Package/Tool Enity Framework
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
dotnet tool install --global dotnet-aspnet-codegenerator

dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools.DotNet --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.Proxies --version 6.0.0

dotnet add package Microsoft.AspNetCore.Session --version 6.0.0
dotnet add package Microsoft.Extensions.Caching.Memory --version 6.0.0
dotnet add package Microsoft.Extensions.DependencyInjection --version 6.0.0
dotnet add package Microsoft.Extensions.Logging --version 6.0.0
dotnet add package Microsoft.Extensions.Logging.Console --version 6.0.0
dotnet add package Bogus --version 35.5.0

## Package hỗ trợ tạo bảng bởi dotnet (dotnet sql cache search on nuget):
dotnet new tool-manifest 
dotnet tool install --local dotnet-sql-cache --version 8.0.0 (yêu cầu phiên bản dotnet 8.0)
dotnet add package Microsoft.Extensions.Caching.SqlServer

- Lệnh tạo bảng nhanh
dotnet sql-cache create "StringConnect" dbo NameTable
string connect : "Data Source=localhost,1433;Initial Catalog=webdb;User ID=SA;Password=Password123; TrustServerCertificate=True;"


## Package về dịch vụ gửi mail
dotnet add package MailKit
dotnet add package MimeKit
dotnet add package RestSharp

## Package Identity
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 6.0.0
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design  --version 6.0.0
dotnet add package Microsoft.AspNetCore.Identity.UI --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication --version 6.0.0
dotnet add package Microsoft.AspNetCore.Http.Abstractions --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Cookies --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Facebook --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Google --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.MicrosoftAccount --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.oAuth --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.OpenIDConnect --version 6.0.0
dotnet add package Microsoft.AspNetCore.Authentication.Twitter --version 6.0.0

## Package elFinder
dotnet add package elFinder.NetCore
 
 - Để làm việc với Migration và Scaffold. Sử dụng lệnh dotnet ef trên Terminal để kiểm tra ef đã cài đặt. Nếu chưa thì cài thêm tool runtime [Download .NET 8.0 Runtime (microsoft.com) ](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime?cid=getdotnetcore&os=windows&arch=x64)

## Cài đặt Package Webpack để đóng gói JS, CSS, SCSS
npm init -y   
npm i -D webpack webpack-cli
npm i node-sass postcss-loader postcss-preset-env 
npm i sass-loader css-loader cssnano 
npm i mini-css-extract-plugin cross-env file-loader
npm install copy-webpack-plugin
npm install npm-watch
npm install bootstrap 
npm install jquery 
npm install popper.js 



