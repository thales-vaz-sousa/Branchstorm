# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .

RUN dotnet publish UserServiceApi/UserServiceApi/UserApi.csproj                          -c Release -o /out/user
RUN dotnet publish TeamServiceApi/TeamServiceApi/TeamApi.csproj                          -c Release -o /out/team
RUN dotnet publish IdeaServiceApi/IdeaServiceApi/IdeaApi.csproj                          -c Release -o /out/idea
RUN dotnet publish HypothesisServiceApi/HypothesisServiceApi/HypothesisServiceApi.csproj -c Release -o /out/hypothesis
RUN dotnet publish FeedbackApi/FeedbackApi/FeedbackApi.csproj                            -c Release -o /out/feedback

# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0
USER root

RUN apt-get update \
 && apt-get install -y --no-install-recommends nginx supervisor gettext-base \
 && rm -rf /var/lib/apt/lists/* /etc/nginx/sites-enabled/default

WORKDIR /app
COPY --from=build /out/user       /app/user
COPY --from=build /out/team       /app/team
COPY --from=build /out/idea       /app/idea
COPY --from=build /out/hypothesis /app/hypothesis
COPY --from=build /out/feedback   /app/feedback

COPY --from=build /src/UserServiceApi/UserServiceApi/Database/             /app/user/Database/
COPY --from=build /src/TeamServiceApi/TeamServiceApi/Database/             /app/team/Database/
COPY --from=build /src/IdeaServiceApi/IdeaServiceApi/Database/             /app/idea/Database/
COPY --from=build /src/HypothesisServiceApi/HypothesisServiceApi/Database/ /app/hypothesis/Database/
COPY --from=build /src/FeedbackApi/FeedbackApi/Database/                   /app/feedback/Database/

COPY deploy/ /etc/gateway/
RUN chmod +x /etc/gateway/start.sh

ENV ASPNETCORE_ENVIRONMENT=Production \
    UserApi__BaseUrl=http://127.0.0.1:5252 \
    TeamApi__BaseUrl=http://127.0.0.1:5236 \
    IdeaApi__BaseUrl=http://127.0.0.1:5221 \
    HypothesisApi__BaseUrl=http://127.0.0.1:5159 \
    FeedbackApi__BaseUrl=http://127.0.0.1:5076 \
    Services__IdeaApiUrl=http://127.0.0.1:5221 \
    DOTNET_GCHeapHardLimit=0x8000000

EXPOSE 10000
CMD ["/etc/gateway/start.sh"]