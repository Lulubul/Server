using AutoMapper;
using Entities;
using NetworkTypes;

namespace Server
{
    public static class AutoMapperConfig
    {
        public static void Initialize()
        {
            Mapper.CreateMap<int, HeroType>().ConvertUsing<HeroTypeConverter>();
            Mapper.CreateMap<int, RaceEnum>().ConvertUsing<RaceConverter>();
            Mapper.CreateMap<int, CreatureType>().ConvertUsing<CreatureTypeConverter>();
            Mapper.CreateMap<Hero, HeroInfo>()
                .ForMember(dest => dest.RaceEnum, opts => opts.MapFrom(src => src.RaceId));
            Mapper.CreateMap<Creature, CreatureInfo>()
                .ForMember(dest => dest.RaceEnum, opts => opts.MapFrom(src => src.RaceId))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.CombatModeId));
            Mapper.CreateMap<Hero, AbstractHero>()
                .ForMember(dest => dest.RaceEnum, opts => opts.MapFrom(src => src.RaceId));
            Mapper.CreateMap<Creature, AbstractCreature>()
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.CombatModeId));
        }
    }

    internal class HeroTypeConverter : ITypeConverter<int, HeroType>
    {
        public HeroType Convert(ResolutionContext context)
        {
            var type = (HeroType) context.SourceValue - 1;
            return type;
        }
    }

    internal class RaceConverter : ITypeConverter<int, RaceEnum>
    {
        public RaceEnum Convert(ResolutionContext context)
        {
            return (RaceEnum)context.SourceValue - 1;
        }
    }

    internal class CreatureTypeConverter : ITypeConverter<int, CreatureType>
    {
        public CreatureType Convert(ResolutionContext context)
        {
            return (CreatureType)context.SourceValue - 1;
        }
    }
}
