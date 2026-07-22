using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class AgentDtoMapperTests
{
    [Fact]
    public void ToReadDto_ExposeCategorieAgentIdCodeEtDescription()
    {
        var agent = new Agent
        {
            IdAgent = 7,
            NomComplet = "Agent Test",
            Matricule = "AT000000001",
            Phone = "0990000001",
            Statut = true,
            CategorieAgentId = 3,
            CategorieAgent = new CategorieAgent
            {
                IdCategorieAgent = 3,
                Code = "AT",
                LibelleCategorie = "Agent de Terrain (AT)",
                Description = "Terrain",
                Statut = true
            }
        };

        var dto = AgentDtoMapper.ToReadDto(agent);

        Assert.Equal(3, dto.CategorieAgentId);
        Assert.Equal("AT", dto.CategorieAgentCode);
        Assert.Equal("Terrain", dto.CategorieAgentDescription);
    }

    [Fact]
    public void ToReadDto_SansCategorie_ChampsNull()
    {
        var agent = new Agent
        {
            IdAgent = 8,
            NomComplet = "Sans Cat",
            Matricule = "XX000000001",
            Phone = "0990000002",
            Statut = true
        };

        var dto = AgentDtoMapper.ToReadDto(agent);

        Assert.Null(dto.CategorieAgentId);
        Assert.Null(dto.CategorieAgentCode);
        Assert.Null(dto.CategorieAgentDescription);
    }
}
