## Running the application

Optionally run the Aspire dashboard

```bash
docker run --rm -it \
    -p 18888:18888 \
    -p 4317:18889 -d \
    --name aspire-dashboard \
    mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

Now run the application from IDE, or use
```dotnetcli
dotnet run
```

Check out telemetry in Aspire dashboard (at http://localhost:18888/traces):