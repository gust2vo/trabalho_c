using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using Products.Data;
using Products.Model;

class HelloWeb
{
    static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApiDocument(config =>
        {
            config.DocumentName = "TodoAPI";
            config.Title = "TodoAPI v1";
            config.Version = "v1";
        });

        builder.Services.AddDbContext<AppDbContext>();

        WebApplication app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseOpenApi();
            app.UseSwaggerUi(config =>
            {
                config.DocumentTitle = "Icecream API";
                config.Path = "/swagger";
                config.DocumentPath = "/swagger/{documentName}/swagger.json";
                config.DocExpansion = "list";
            });
        }   

        app.MapPost("/createProduct", (AppDbContext context , string nameProduct, float price, int amount) =>{
            var newProduct = new Product(Guid.NewGuid(), nameProduct, price, amount);
            context.products.Add(newProduct);
            context.SaveChanges();
            return Results.Ok(newProduct);
        }).Produces<Product>();
        
        app.MapGet("/allProducts", (AppDbContext context) =>{
            var prods = context.products;
            return prods is not null ? Results.Ok(prods) : Results.NotFound(); 
        }).Produces<Product>();

        app.MapGet("/idProducts", (AppDbContext context , Guid inputId) =>{
            var prodsId = context.products.Find(inputId);
            return prodsId is not null ? Results.Ok(prodsId) : Results.NotFound();
        }).Produces<Product>();

        app.MapGet("/nameProducts", (AppDbContext context , String inputName) =>{
            var prodName = context.products.Where(p => p.nameProduct == inputName);
            return prodName is not null ? Results.Ok(prodName) : Results.NotFound();
        }).Produces<Product>();

        app.MapPatch("/patch", (AppDbContext context, Guid id, [FromBody] string newName) =>
        {
        var prodToUpdate = context.products.Find(id);
            if (prodToUpdate == null){
                return Results.NotFound();
            }

        
        context.Entry(prodToUpdate).State = EntityState.Detached;

        
        var updatedProduct = new Product(id, newName, prodToUpdate.price, prodToUpdate.amount);

        context.products.Update(updatedProduct);
        context.SaveChanges();

            return Results.Ok(updatedProduct);
        }).Produces<Product>();

        app.MapPut("/putProduct", (AppDbContext context , Product inputId) =>{
            var prodUpdate = context.products.Find(inputId.id);
            if (prodUpdate == null) {
                return Results.NotFound();
            }
            var entry = context.Entry(prodUpdate).CurrentValues;
            entry.SetValues(inputId);
            context.SaveChanges();
            return Results.Ok(inputId);
        }).Produces<Product>();

        app.MapDelete("/deleteProduct", (AppDbContext context, Guid id) =>{
           var prodDelete = context.products.Find(id);
           if (prodDelete == null) {
                return Results.NotFound();
           }
           context.products.Remove(prodDelete);
           context.SaveChanges();
           return Results.Ok(prodDelete);
        }).Produces<Product>();

        app.Run();
    }
}