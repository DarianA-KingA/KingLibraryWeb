using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KingLibraryWeb.Utility
{
    public static class SD //Static Details
    {
        #region Roles list
        public const string Role_User_Indi = "Individual";
        public const string Role_User_Comp = "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        #endregion
        #region Order and payment status
        # region OrderStatus
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusProccessing = "Proccessing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";
        #endregion
        # region PaymentStatus
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "AprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";
        #endregion
        #region SessionIdentifiers
        public const string SessionCart = "SessionShoppingCart";
        #endregion
        #endregion

    }
}
