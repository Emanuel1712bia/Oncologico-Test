namespace OrderProcessing.Application.Products.Dtos;

public sealed record ProductResponse(Guid Id, string Name, string Description, string Indication, decimal Price);
