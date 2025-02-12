// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Contracts;

namespace Fabric.Rti.workload.Backend.Items
{
    public interface IItem1 : IItem
    {
        ItemReference Lakehouse { get; }

        int Operand1 { get; }

        int Operand2 { get; }
        
        /// <summary>
        /// Doubles the operands produced by the item calculation.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The result is a tuple containing the doubled Operand1 and Operand2.</returns>
        Task<(int Operand1, int Operand2)> Double();
    }
}
