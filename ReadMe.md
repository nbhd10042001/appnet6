dotnet new mvc -o AppMVC --use-program-main

- View (on top Visual Code)
- Command Palette...
- type input : >.NET: Generate Assests for Build and Debug

> dotnet --list-sdks
> dotnet --list-runtimes
> dotnet list package
> dotnet restore

> dotnet publish -c Release -o app/publish
> dotnet App.dll

# command kill process linux-centos
ps -ef | grep dotnet
kill -9 <PID>
which dotnet 


# appsetting.json
"Kestrel": {
"Endpoints": {
    "Http": {
    "Url": "http://localhost:5004"
    }
}
},

# Nâng cao lệnh xóa 1 máy ảo cụ thể:
Check available installed boxes by calling
> vagrant box list
Find box id
> vagrant global-status --prune
Select by id name of your box for destroying.
> vagrant destory id

# Nội dung soạn thảo của chương trình VI có tên appmvc.service (service tự động reset khi bị crash)
[Unit]
Description=Ung dung AppMVC
[Service]
WorkingDirectory=/home/aspnet/publish/
ExecStart=/usr/bin/dotnet /home/aspnet/publish/App.dll
Restart=always
# Khởi động lại ứng dụng sau 10s bị crash
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=asp-net-app
User=root
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target


# Nội dung thiết lập máy chủ Apache
<VirtualHost *:*>
    RequestHeader set "X-Forwarded-Proto" expr="%{REQUEST_SCHEME}e"
</VirtualHost>

<VirtualHost *:80>
    # ProxyPreserveHost On
    # ProxyPass / http://127.0.0.1:5000/
    # ProxyPassReverse / http://127.0.0.1:5000/
    ServerName appmvc.net
    ServerAlias *.appmvc.net

    # chuyển hướng http sang https
    RewriteEngine On
    RewriteCond %{HTTPS} !=on
    RewriteRule ^/?(.*) https://%{SERVER_NAME}/$1 [R,L]
</VirtualHost>

<VirtualHost *:443>
    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:5000/
    ProxyPassReverse / http://127.0.0.1:5000/
    ServerName appmvc.net
    ServerAlias *.appmvc.net
    
    # Cấu hình HTTPS
    SSLEngine on
    # SSLProtocol all -SSLv2
    # SSLCipherSuite ALL:!ADH:!EXPORT:!SSLv2:!RC4+RSA:+HIGH:+MEDIUM:!LOW:!RC4
    SSLCertificateFile /certtest/ca.crt
    SSLCertificateKeyFile /certtest/ca.key
</VirtualHost>



# Nội dung cấu hình nginx.conf
upstream mvcapp {
    server localhost:5000;
}

server {
    listen        80;
    server_name   appmvc.net *.appmvc.net;

    # location / {
    #     proxy_pass         http://localhost:5000;
    #     proxy_http_version 1.1;
    #     proxy_set_header   Upgrade $http_upgrade;
    #     proxy_set_header   Connection keep-alive;
    #     proxy_set_header   Host $host;
    #     proxy_cache_bypass $http_upgrade;
    #     proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
    #     proxy_set_header   X-Forwarded-Proto $scheme;
    # }

    # chuyen huong ve https
    add_header Strict-Transport-Security max-age=15768000;
    return     301 https://$host$request_uri;
}

server {
    listen                    *:443 ssl;
    server_name               appmvc.net;
    ssl_certificate           /certtest/ca.crt;
    ssl_certificate_key       /certtest/ca.key;
    ssl_protocols             TLSv1.1 TLSv1.2;
    ssl_prefer_server_ciphers on;
    ssl_ciphers               "EECDH+AESGCM:EDH+AESGCM:AES256+EECDH:AES256+EDH";
    ssl_ecdh_curve            secp384r1;
    ssl_session_cache         shared:SSL:10m;
    ssl_session_tickets       off;
    ssl_stapling              on; #ensure your cert is capable
    ssl_stapling_verify       on; #ensure your cert is capable

    add_header Strict-Transport-Security "max-age=63072000; includeSubdomains; preload";
    add_header X-Frame-Options DENY;
    add_header X-Content-Type-Options nosniff;

    #Redirects all traffic
    location / {
        proxy_pass http://mvcapp;
        limit_req  zone=one burst=10 nodelay;
    }
}

# Nội dung cấu hình file proxy.conf
proxy_redirect          off;
proxy_set_header        Host $host;
proxy_set_header        X-Real-IP $remote_addr;
proxy_set_header        X-Forwarded-For $proxy_add_x_forwarded_for;
proxy_set_header        X-Forwarded-Proto $scheme;
client_max_body_size    10m;
client_body_buffer_size 128k;
proxy_connect_timeout   90;
proxy_send_timeout      90;
proxy_read_timeout      90;
proxy_buffers           32 4k;


# Nội dung chèn vào đầu khối http trong file nginx.conf để nạp file proxy.conf vào:
include        /etc/nginx/proxy.conf;
limit_req_zone $binary_remote_addr zone=one:10m rate=5r/s;
server_tokens  off;

client_body_timeout 10; client_header_timeout 10; send_timeout 10;