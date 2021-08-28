using System;
using System.Collections.Generic;
using SourceMapper.Test.Entities;
using Xunit;
using SourceMapper;
using SourceMapper.Test.Models;

namespace SourceMapper.Test
{
    public class MapEnumerable
    {
        private static readonly List<User> Sources = new()
        {
            new User
            {
                Name = "Foo",
                Username = "x_foo_x"
            },
            new User
            {
                Name = "Bar",
                Username = "bar21"
            }
        };

        [Fact]
        public void TestMap()
        {
            Assert.Equal(new[]
            {
                new UserDto
                {
                    Username = "x_foo_x"
                },
                new UserDto
                {
                    Username = "bar21"
                }
            }, Sources.ToUserDto());
        }

        [Fact]
        public void TestEmptyFactory()
        {
            Assert.Equal(new[]
            {
                new UserDtoWithFactory
                {
                    Name = "Foo",
                    Username = "x_foo_x"
                },
                new UserDtoWithFactory
                {
                    Name = "Bar",
                    Username = "bar21"
                }
            }, Sources.ToUserDtoWithFactory());
        }

        [Fact]
        public void TestOptionalArgument()
        {
            Assert.Equal(new[]
            {
                new UserDtoWithFactory
                {
                    Name = "x_foo_x",
                    Username = "x_foo_x"
                },
                new UserDtoWithFactory
                {
                    Name = "bar21",
                    Username = "bar21"
                }
            }, Sources.ToUserDtoWithFactory(false));
        }
    }
}
