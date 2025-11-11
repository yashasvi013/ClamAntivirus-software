using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Added namespace for IFormFile

namespace InsurancePolicyForm.Models
{
    public class InsuranceDeclarationModel
    {
        [Required(ErrorMessage = "Insured Address is required")]
        [Display(Name = "Insured Address/Unit")]
        public string InsuredAddress { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Effective Date is required")]
        [Display(Name = "Effective Date")]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; }

        [Required(ErrorMessage = "Expiration Date is required")]
        [Display(Name = "Expiration Date")]
        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [Display(Name = "Lease Start Date")]
        [DataType(DataType.Date)]
        public DateTime? LeaseStartDate { get; set; }

        [Required(ErrorMessage = "Insurance Provider is required")]
        [Display(Name = "Insurance Provider")]
        public string InsuranceProvider { get; set; }

        [Required(ErrorMessage = "Policy Number is required")]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        [Required(ErrorMessage = "Liability Coverage Amount is required")]
        [Display(Name = "Liability Coverage Amount")]
        [DataType(DataType.Currency)]
        [Range(100000, double.MaxValue, ErrorMessage = "Coverage must be at least $100,000")]
        public decimal LiabilityCoverageAmount { get; set; }

        [Required(ErrorMessage = "Please certify that all details are accurate")]
        [Display(Name = "I certify that all details provided above are accurate to my knowledge")]
        public bool CertifyAccuracy { get; set; }

        [Required(ErrorMessage = "Please confirm that your lease is signed")]
        [Display(Name = "I confirm that my lease agreement is duly signed")]
        public bool ConfirmLeaseSigned { get; set; }

        [Display(Name = "I confirm that I have added the PO Box address of this property as an interested party")]
        public bool AddedPoBoxAddress { get; set; }

        // File upload property for document scanning
        [Display(Name = "Insurance Declaration Document")]
        public IFormFile? UploadedDocument { get; set; }
    }
}
