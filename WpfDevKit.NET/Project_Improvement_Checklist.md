
# ✅ Project-Wide Improvement Checklist

---

## 🔧 Dependency Injection

- [ ] Scoped Lifetime Support (✳️ skipped intentionally)
- [ ] TryAdd, TryAddEnumerable support
- [ ] `GetService<T>()` ✅ already exists
- [ ] Open Generic Registration (e.g., IRepo<> → Repo<>)
- [ ] Thread-safe singleton instantiation (double-checked locking)
- [ ] Constructor caching for performance
- [ ] Optional: Lazy<T>, Func<T> resolution

## 🏭 Object Factory

- [ ] Support interface registration for [Resolvable] classes
- [ ] Cache best constructor per type
- [ ] Add [Resolvable(Lifetime = Singleton)] support
- [ ] Improve constructor error diagnostics

## 🏗 Hosting & Background Services

- [x] StopAsync() in ServiceHost
- [x] Dispose() calls StopAsync() with timeout
- [x] BackgroundService.ExecuteAsync() wrapped in try/catch
- [ ] Flush/dispose log queues during shutdown
- [x] Graceful Dispose() with wait
- [x] Implement IApplicationLifetime
- [ ] Allow OnStarted, OnStopping callbacks for lifecycle hooks

## 🪵 Logging

### Core

- [x] Queue + async background writer
- [x] Multi-provider pipeline
- [x] Strong metrics support
- [ ] Add CancellationToken to LogAsync
- [ ] Add FlushAsync() for providers
- [ ] Optional ILogFormatter/template engine
- [ ] Propagate LogException internally for unhandled logs

### Providers

- [x] Category-based filters
- [x] Console formatting/colors
- [x] Memory/Database/User/gRPC providers
- [ ] Support provider Replace() or update
- [ ] Add provider lifecycle hooks (OnAdd, OnRemove)
- [ ] Optional: interface-based ILogFormatter reuse

### Collection

- [ ] Match on instance, not just provider.GetType()
- [ ] Improve thread safety (lock granularity)

## 📡 Connectivity

- [ ] Surface LastErrorMessage in ConnectivityService
- [ ] Reduce duplicate ConnectionChanged events
- [ ] Ensure IsReadyAsync() throws if cancelled

## 📂 Remote File Access

- [ ] Add IRemoteFileConnection interface
- [ ] Add async-wrapped version (CreateAsync)
- [ ] Optional: internal ILogger for exceptions
- [ ] Optional: reference counting to avoid premature disconnect

## 🔁 Utility / Base Classes

- [ ] Add BusyCount to IBusyService
- [x] Fire IsBusyChanged on UI thread
- [ ] Improve EnumExtensions via caching
- [ ] ToReadableTime() precision overload
- [ ] Add unit tests for:
  - EnumExtensions
  - ExponentialRetry
  - TimeSpanExtensions
  - StartStopRegistration
