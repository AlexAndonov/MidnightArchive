using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using MidnightArchive.Core.Mappings;

namespace MidnightArchive.Tests.Helpers
{
	public static class MapperFactory
	{
		public static IMapper Create()
		{
			var config = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<StoryProfile>();
				cfg.AddProfile<CommentProfile>();
				cfg.AddProfile<EventProfile>();
				cfg.AddProfile<CategoryProfile>();
			}, NullLoggerFactory.Instance);

			config.AssertConfigurationIsValid();

			return config.CreateMapper();
		}
	}
}