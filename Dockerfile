# ------------------------------
# 第一階段：搬運工 (Build Stage)
# 我們使用含有 .NET SDK (工具箱) 的映像檔來編譯程式
# ------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. 先只複製 csproj (專案檔)
# 為什麼？因為這樣可以利用 Docker 快取，如果你沒加新套件，還原速度會飛快
COPY ["ECommerceBackend/ECommerceBackend.csproj", "ECommerceBackend/"]

# 2. 下載所有依賴套件 (Restore)
RUN dotnet restore "ECommerceBackend/ECommerceBackend.csproj"

# 3. 複製剩下的所有程式碼
COPY . .

# 4. 開始編譯並發布 (Publish)
# 這會產出最終的 .dll 檔案到 /app/publish 資料夾
WORKDIR "/src/ECommerceBackend"
RUN dotnet publish "ECommerceBackend.csproj" -c Release -o /app/publish

# ------------------------------
# 第二階段：執行者 (Runtime Stage)
# 這裡改用只有 Runtime (執行環境) 的輕量級映像檔
# 這樣的做出來的 Image 會很小，省空間
# ------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# 把第一階段做好的 /app/publish 搬過來這裡
COPY --from=build /app/publish .

# 告訴 Docker：啟動時請執行這個指令
ENTRYPOINT ["dotnet", "ECommerceBackend.dll"]