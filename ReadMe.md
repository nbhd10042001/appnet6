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


- Nâng cao lệnh xóa 1 máy ảo cụ thể:
    Check available installed boxes by calling
    > vagrant box list
    Find box id
    > vagrant global-status --prune
    Select by id name of your box for destroying.
    > vagrant destory id