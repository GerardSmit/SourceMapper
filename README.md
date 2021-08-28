# SourceMapper
SourceMapper is source generator that maps one object to another.

## Installation
Currently there are no NuGet packages because the project is in early stage.

## Roadmap
- [x] Factories
- [ ] Type conversions
- [ ] Nested mappings

## Example
In the following example, we map the `Username` from `User` to `Username` in `UserDto`.

**Source**
```cs
class User
{
    public string Username { get; set; }
}
```

**Target**
```cs
[MapFrom(typeof(User))]
class UserDto
{
    public string Username { get; set; }
}
```

**Generated code**
```cs
/// <summary>
/// Automatically generated mapper for <see cref="UserDto"/>.
/// To stop generating this class, remove attribute <see cref="MapFromAttribute"/> from <see cref="UserDto"/>.
/// </summary>
[DebuggerStepThrough]
[GeneratedCodeAttribute("SourceMapper", "1.0.0.0")]
public static class UserDtoMapper
{
    #region No Factory

    public static readonly Func<User, UserDto> NewFunc = Map;
    public static readonly Expression<Func<User, UserDto>> NewExpression = source => new UserDto()
    {
        Username = source.Username,
    };

    public static UserDto Map(User source) => new UserDto()
    {
        Username = source.Username,
    };

    public static UserDto ToUserDto(this User x) => Map(x);
    public static IQueryable<UserDto> ToUserDto(this IQueryable<User> query) => query.Select(NewExpression);
    public static IEnumerable<UserDto> ToUserDto(this IEnumerable<User> query) => query.Select(NewFunc);

    #endregion
}
```

### Factories
It's also possible to create a factory by adding a static method called `Map` in `UserDto`.  
This way you can create your own property mappings while SourceGenerator auto maps the properties that you didn't provide.

For example, we provide `Name` but not `Username`. In this case SourceGenerator auto generates a method that'll set this propery.

In the factory, it's also possible to create variables. When using the source type (`User` in this case) they'll be translated into expressions.

**Source**
```cs
class User
{
    public string Username { get; set; }

    public string Name { get; set; }
}
```

**Target**
```cs
[MapFrom(typeof(User))]
class UserDto
{
    public string Username { get; set; }

    public string Name { get; set; }

    internal static UserDto Map(User user, bool showName, bool uppercaseName = false)
    {
        var name = uppercaseName ? user.Name.ToUpper() : user.Name;

        return new UserDto
        {
            Name = showName ? name : user.Username
        };
    }
}
```

**Generated code**  
In the generated code, there are now two classes:  
`Params_10856f`: stores the parameters of the method.  
`Vars_10856f`: stores the variables and parameters.

```cs
/// <summary>
/// Automatically generated mapper for <see cref="UserDto"/>.
/// To stop generating this class, remove attribute <see cref="MapFromAttribute"/> from <see cref="UserDto"/>.
/// </summary>
[DebuggerStepThrough]
[GeneratedCodeAttribute("SourceMapper", "1.0.0.0")]
public static class UserDtoMapper
{
    #region Factory Map(User user, Boolean showName, Boolean uppercaseName)

    private static readonly ParameterExpression e_user_10856f = Expression.Parameter(typeof(User), "user");
    private static readonly Type t_10856f = typeof(Params_10856f);
    private static readonly PropertyInfo p_user_10856f = t_10856f.GetProperty("user");
    private static readonly PropertyInfo p_showName_10856f = t_10856f.GetProperty("showName");
    private static readonly PropertyInfo p_uppercaseName_10856f = t_10856f.GetProperty("uppercaseName");

    private class Params_10856f
    {
        public User user { get; set; } 
        public Boolean showName { get; set; } 
        public Boolean uppercaseName { get; set; } 
    }

    private class Vars_10856f
    {
        public User user { get; set; } 
        public Boolean showName { get; set; } 
        public Boolean uppercaseName { get; set; } 
        public String name { get; set; } 
    }

    private static readonly Expression<Func<Params_10856f, Vars_10856f>> VarsExpression_10856f = _params => new Vars_10856f
    {
        user = _params.user,
        showName = _params.showName,
        uppercaseName = _params.uppercaseName,
        name = _params.uppercaseName ? _params.user.Name.ToUpper() : _params.user.Name,
    };

    private static readonly Expression<Func<Vars_10856f, UserDto>> Expression_10856f = _params => new UserDto
    {
        Name = _params.showName ? _params.name : _params.user.Username,
        Username = _params.user.Username,
    };

    public static UserDto Map(User user, Boolean showName, Boolean uppercaseName)
    {
        var result = UserDto.Map(user, showName, uppercaseName);
        result.Username = user.Username;
        return result;
    }

    public static IQueryable<UserDto> ToUserDto(this IQueryable<User> query, Boolean showName, Boolean uppercaseName = false)
    {
        return query
            .Select(Expression.Lambda<Func<User, Params_10856f>>(
                Expression.MemberInit(
                    Expression.New(t_10856f),
                    new MemberBinding[]
                    {
                        Expression.Bind(p_user_10856f, e_user_10856f),
                        Expression.Bind(p_showName_10856f, Expression.Constant(showName)),
                        Expression.Bind(p_uppercaseName_10856f, Expression.Constant(uppercaseName)),
                    }
                ),
                e_user_10856f
            ))
            .Select(VarsExpression_10856f)
            .Select(Expression_10856f);
    }

    public static IEnumerable<UserDto> ToUserDto(this IEnumerable<User> query, Boolean showName, Boolean uppercaseName = false) => query
        .Select(x => Map(x, showName, uppercaseName));

    public static UserDto ToUserDto(this User user, Boolean showName, Boolean uppercaseName = false) =>
        Map(user, showName, uppercaseName);

    #endregion
}
```