using MediatR;
using DemoProject.Models;

namespace DemoProject.Features.Query;

public class GetAllUserQuery:IRequest<List<User>> { }