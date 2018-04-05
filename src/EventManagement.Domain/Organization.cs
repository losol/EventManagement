using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace losol.EventManagement.Domain
{


    public class Organization
    {
        public int OrganizationId { get; set; }
        
        [Required]
        public string Name {get;set;} 

        [StringLength(300, ErrorMessage = "Beskrivelsen kan bare være 300 tegn.")]
        [Display(Name = "Kort beskrivelse av organisasjonen.")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "Lenken kan bare være 300 tegn.")]
        [Display(Name = "Lenke til organisasjonens nettsted")]
        public string Url { get; set; }
        
        [StringLength(300, ErrorMessage = "Telefonnummeret kan bare være 50 tegn.")]
        [Display(Name = "Telefon til organisasjonen.")]
        public string Phone { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "Eposten kan bare være 300 tegn.")]
        [Display(Name = "Epost til organisasjonen.")]
        public string Email { get; set; }

        [StringLength(300, ErrorMessage = "Url kan bare være 300 tegn.")]
        [Display(Name = "Lenke til profilbilde for organisasjonen.")]
        public string LogoImageUrl { get; set; }

        public string VatId { get; set; }
 
       }
}