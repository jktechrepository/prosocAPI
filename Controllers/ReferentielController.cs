using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProsocAPI.Models.Core;
using ProsocAPI.Models.DTOs.Core;

namespace ProsocAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferentielController : ControllerBase
    {
        /// <summary>
        /// Liste des liens de parenté acceptés (codes, libellés, catégories) pour personne de contact, dépendants et bénéficiaires.
        /// </summary>
        [HttpGet("liens-parente")]
        [AllowAnonymous]
        public ActionResult<LienParenteReferentielDto> GetLiensParente()
        {
            var referentiel = new LienParenteReferentielDto
            {
                Liens = LienParenteRegles.GetReferentiel()
                    .Select(e => new LienParenteReferentielItemDto
                    {
                        Code = e.Code,
                        Libelle = e.Libelle,
                        Categorie = e.Categorie
                    })
                    .ToList(),
                LiensEnfant = LienParenteRegles.LiensEnfant,
                LiensConjoint = LienParenteRegles.LiensConjoint
            };

            return Ok(referentiel);
        }
    }
}
