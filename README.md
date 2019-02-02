# Introduction

This library adds extensions to the service collection that add dependencies with specific construction options.
This extends the Microsoft's Dependency Injection Extentensions without replacing it.

```
var services = new ServiceCollection();

var provider = services
    .AddTransient<DependencyA>()
    .AddTransient<DependencyB>()
    .AddTransient<DependencyC>()
    .AddTransient<ISimple, Simple>()
    .AddTransientWithOptions<MyService>(o => o.ConstructWith<IDependency, DependencyB>())
    .WriteDiagnostics()
    .BuildServiceProvider();

var myService = provider.GetService<MyService>();
```
