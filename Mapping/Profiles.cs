using AutoMapper;
using NepalDictLib.Models;

namespace NepalDictServer.Mapping
{
    public class ModelToResourceProfile : Profile
    {
        public ModelToResourceProfile()
        {
            CreateMap<UserModel, UserResource>();
        }
    }

    public class ResourceToModelProfile : Profile
    {
        public ResourceToModelProfile()
        {
            CreateMap<UserResource, UserModel>();
        }
    }
}
