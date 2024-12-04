using Amazon.Rekognition.Model;
using AutoMapper;
using Share.Model;

namespace FAL.Utils
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Face, FaceTrainModel>()
                .ForMember(src => src.FaceId, dest => dest.MapFrom(x => x.FaceId))
                .ForMember(src => src.UserId, dest => dest.MapFrom(x => x.UserId))
                .ReverseMap();
        }
    }
}
