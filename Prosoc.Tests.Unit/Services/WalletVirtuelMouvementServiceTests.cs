using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Prosoc.Data;
using ProsocAPI.Models.Authentication;
using ProsocAPI.Models.Core;
using ProsocAPI.Services;

namespace Prosoc.Tests.Unit.Services;

public class WalletVirtuelMouvementServiceTests
{
    [Fact]
    public async Task EnregistrerMouvementAsync_PersisteChampsEnrichis()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>().UseSqlite(connection).Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var operateur = new Utilisateur { NomUtilisateur = "Financier", MotDePasseHash = "hash", Statut = true };
        db.Utilisateurs.Add(operateur);
        var agent = new Agent { NomComplet = "AT", Matricule = "AT000000099", Phone = "0990000099", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            SoldeVirtuel = 100m,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        var service = new WalletVirtuelMouvementService(db);
        await service.EnregistrerMouvementAsync(
            wallet.IdWalletVirtuelAgent,
            50m,
            "CREDIT",
            WalletVirtuelMouvementSources.AjoutSolde,
            100m,
            150m,
            operateur.IdUtilisateur,
            devise.IdDevise,
            "Recharge test");
        await db.SaveChangesAsync();

        var mouvement = await db.WalletVirtuelMouvements.SingleAsync();
        Assert.Equal(50m, mouvement.Montant);
        Assert.Equal("CREDIT", mouvement.TypeOperation);
        Assert.Equal(WalletVirtuelMouvementSources.AjoutSolde, mouvement.Source);
        Assert.Equal(100m, mouvement.SoldeAvant);
        Assert.Equal(150m, mouvement.SoldeApres);
        Assert.Equal(operateur.IdUtilisateur, mouvement.OperateurUtilisateurId);
        Assert.Equal(devise.IdDevise, mouvement.DeviseId);
        Assert.Equal("Recharge test", mouvement.Description);
    }

    [Fact]
    public async Task EnregistrerDeltaSoldeAsync_CreditPositif_PersisteSoldes()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ProsocDbContext>().UseSqlite(connection).Options;
        await using var db = new ProsocDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var devise = new Devise { Code = "USD", Nom = "Dollar", EstDevisePrincipale = true, Statut = true };
        db.Devises.Add(devise);
        var agent = new Agent { NomComplet = "AT2", Matricule = "AT000000098", Phone = "0990000098", Statut = true };
        db.Agents.Add(agent);
        await db.SaveChangesAsync();

        var wallet = new WalletVirtuelAgent
        {
            AgentId = agent.IdAgent,
            DeviseId = devise.IdDevise,
            SoldeVirtuel = 200m,
            Statut = true
        };
        db.WalletsVirtuelsAgents.Add(wallet);
        await db.SaveChangesAsync();

        var service = new WalletVirtuelMouvementService(db);
        await service.EnregistrerDeltaSoldeAsync(
            wallet.IdWalletVirtuelAgent,
            200m,
            250m,
            WalletVirtuelMouvementSources.AjustementSolde,
            deviseId: devise.IdDevise);
        await db.SaveChangesAsync();

        var mouvement = await db.WalletVirtuelMouvements.SingleAsync();
        Assert.Equal("CREDIT", mouvement.TypeOperation);
        Assert.Equal(50m, mouvement.Montant);
        Assert.Equal(200m, mouvement.SoldeAvant);
        Assert.Equal(250m, mouvement.SoldeApres);
    }
}
