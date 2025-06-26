using PCBuildAssistant.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddScoped<IAzureOpenAIService, AzureOpenAIService>();
builder.Services.AddScoped<IPCBuildService, PCBuildService>();

// Add HttpClient for external API calls
builder.Services.AddHttpClient();

// Configure HttpClient for Blazor components with dynamic base address
builder.Services.AddScoped(sp =>
{
    var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
    var baseAddress = httpContextAccessor?.HttpContext?.Request != null
        ? $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}"
        : "http://localhost:5000";
    return new HttpClient { BaseAddress = new Uri(baseAddress) };
});

// Add HttpContextAccessor for dynamic base address
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllers(); // Map API controllers first
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host"); // Map fallback last

// Bind to all interfaces for container compatibility
app.Run("http://0.0.0.0:5000");
