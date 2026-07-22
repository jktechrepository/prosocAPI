using OfficeOpenXml;
using ProsocAPI.Models.DTOs.Core;
using ProsocAPI.Models.Pagination;
using ProsocAPI.Services.Repositories;

namespace ProsocAPI.Services
{
    public class PerceptionVirtuelleExportService : IPerceptionVirtuelleExportService
    {
        private const int MaxExportRows = 10_000;
        private readonly IDashboardPercepteurRepository _dashboardPercepteur;

        public PerceptionVirtuelleExportService(IDashboardPercepteurRepository dashboardPercepteur)
        {
            _dashboardPercepteur = dashboardPercepteur;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> ExportRapportAsync(
            DateTime? dateDebut,
            DateTime? dateFin,
            string? origine,
            string? statut,
            int? agentId,
            int? affilieId,
            CancellationToken ct = default)
        {
            var rapport = await _dashboardPercepteur.GetRapportPerceptionAsync(
                dateDebut,
                dateFin,
                origine,
                statut,
                agentId,
                affilieId,
                new PaginationRequest { Page = 1, PageSize = MaxExportRows },
                ct);

            using var package = new ExcelPackage();

            var syntheseSheet = package.Workbook.Worksheets.Add("Synthese");
            syntheseSheet.Cells[1, 1].Value = "Canal";
            syntheseSheet.Cells[1, 2].Value = "Montant en attente";
            syntheseSheet.Cells[1, 3].Value = "Nombre en attente";
            syntheseSheet.Cells[1, 4].Value = "Montant perçu";
            syntheseSheet.Cells[1, 5].Value = "Nombre perçu";

            syntheseSheet.Cells[2, 1].Value = "Agent (VA)";
            syntheseSheet.Cells[2, 2].Value = rapport.Synthese.Agent.MontantEnAttente;
            syntheseSheet.Cells[2, 3].Value = rapport.Synthese.Agent.NombreEnAttente;
            syntheseSheet.Cells[2, 4].Value = rapport.Synthese.Agent.MontantPerçu;
            syntheseSheet.Cells[2, 5].Value = rapport.Synthese.Agent.NombrePerçu;

            syntheseSheet.Cells[3, 1].Value = "Affilié (guichet)";
            syntheseSheet.Cells[3, 2].Value = rapport.Synthese.Affilie.MontantEnAttente;
            syntheseSheet.Cells[3, 3].Value = rapport.Synthese.Affilie.NombreEnAttente;
            syntheseSheet.Cells[3, 4].Value = rapport.Synthese.Affilie.MontantPerçu;
            syntheseSheet.Cells[3, 5].Value = rapport.Synthese.Affilie.NombrePerçu;

            syntheseSheet.Cells[5, 1].Value = "Total perçu";
            syntheseSheet.Cells[5, 2].Value = rapport.Synthese.TotalPerçu;
            syntheseSheet.Cells[5, 3].Value = rapport.Synthese.DeviseCode;

            var lignesSheet = package.Workbook.Worksheets.Add("Lignes");
            var headers = new[]
            {
                "Origine", "Statut", "IdCollecte", "Montant", "MontantDevisePrincipale", "Devise",
                "AffilieId", "AffilieNom", "AgentId", "AgentNom", "AgentMatricule", "ModePaiement",
                "DateCollecte", "DatePerception", "PerceptionVirtuelleId", "WalletVirtuelMouvementId",
                "PercepteurNom", "ReferencePaiement", "Observation"
            };

            for (var col = 0; col < headers.Length; col++)
                lignesSheet.Cells[1, col + 1].Value = headers[col];

            var row = 2;
            foreach (var ligne in rapport.Lignes.Data)
            {
                lignesSheet.Cells[row, 1].Value = ligne.OriginePerception;
                lignesSheet.Cells[row, 2].Value = ligne.StatutPerception;
                lignesSheet.Cells[row, 3].Value = ligne.IdCollecte;
                lignesSheet.Cells[row, 4].Value = ligne.Montant;
                lignesSheet.Cells[row, 5].Value = ligne.MontantDevisePrincipale;
                lignesSheet.Cells[row, 6].Value = ligne.DeviseCode;
                lignesSheet.Cells[row, 7].Value = ligne.AffilieId;
                lignesSheet.Cells[row, 8].Value = ligne.AffilieNom;
                lignesSheet.Cells[row, 9].Value = ligne.AgentId;
                lignesSheet.Cells[row, 10].Value = ligne.AgentNom;
                lignesSheet.Cells[row, 11].Value = ligne.AgentMatricule;
                lignesSheet.Cells[row, 12].Value = ligne.ModePaiement;
                lignesSheet.Cells[row, 13].Value = ligne.DateCollecte;
                lignesSheet.Cells[row, 14].Value = ligne.DatePerception;
                lignesSheet.Cells[row, 15].Value = ligne.PerceptionVirtuelleId;
                lignesSheet.Cells[row, 16].Value = ligne.WalletVirtuelMouvementId;
                lignesSheet.Cells[row, 17].Value = ligne.PercepteurNom;
                lignesSheet.Cells[row, 18].Value = ligne.ReferencePaiement;
                lignesSheet.Cells[row, 19].Value = ligne.Observation;
                row++;
            }

            syntheseSheet.Cells.AutoFitColumns();
            lignesSheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }
    }
}
