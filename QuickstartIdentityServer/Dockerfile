FROM microsoft/dotnet:2.2-aspnetcore-runtime
EXPOSE 80

#定义时区参数
ENV TZ=Asia/Shanghai
 
#设置时区
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo '$TZ' > /etc/timezone

WORKDIR /qcc
COPY . /qcc

ENTRYPOINT ["dotnet", "QuickstartIdentityServer.dll"]