using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaccineFinder
{

    public class GenerateMobileOTPResponse
    {
        public string txnId { get; set; }
    }

    public class ValidateMobileOTPResponse
    {
        public string token { get; set; }
        public string isNewAccount { get; set; }
    }

    public class GetBeneficiariesResponse
    {
        public List<Beneficiary> beneficiaries { get; set; }
    }

    public class Beneficiary
    {
        public string beneficiary_reference_id { get; set; }
        public string name { get; set; }
        public string birth_year { get; set; }
        public string gender { get; set; }
        public string mobile_number { get; set; }
        public string photo_id_type { get; set; }
        public string photo_id_number { get; set; }
        public string comorbidity_ind { get; set; }
        public string vaccination_status { get; set; }
        public string vaccine { get; set; }
        public string dose1_date { get; set; }
        public string dose2_date { get; set; }
        public List<object> appointments { get; set; }
        
        public string Description
        {
            get
            {
                return name + " : " + beneficiary_reference_id;
            }
        }
    }


    class AvailabilityStatusAPIResponse
    {
        public List<Center> centers { get; set; }
        public bool SessionRelatedError { get; set; }
    }
    public class Center
    {
        public int center_id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string state_name { get; set; }
        public string district_name { get; set; }
        public string block_name { get; set; }
        public int pincode { get; set; }
        public int lat { get; set; }
        public int @long { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string fee_type { get; set; }
        public List<Session> sessions { get; set; }
    }

    public class Session
    {
        public string session_id { get; set; }
        public string date { get; set; }
        public int available_capacity { get; set; }
        public int min_age_limit { get; set; }
        public string vaccine { get; set; }
        public List<string> slots { get; set; }
        public int available_capacity_dose1 { get; set; }
        public int available_capacity_dose2 { get; set; }
    }

    public class SlotBookingResponse
    {
        public string appointment_confirmation_no { get; set; }
    }
}
