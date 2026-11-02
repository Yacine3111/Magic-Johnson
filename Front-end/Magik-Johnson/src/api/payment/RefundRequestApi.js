import { apiClient } from '@/api/Axios'

export const CreateRefundRequest = async (request) => {
  const response = await apiClient.post('/refundRequest', request)
  return response.data
}

export const GetRefundRequest = async () => {
  const response = await apiClient.get('/refundRequest/getRefundRequests')
  return response.data
}

export const ProcessRefundRequest = async (item, newStatus) => {
  const response = await apiClient.post('/payment/refund', {
    RefundRequestId: item.id,
    PaymentIntentId: item.paymentIntentId,
    OrderId: item.orderId,
    IsPartial: item.isPartialRefund,
    IsAccepted: newStatus == 'acceptée' ? true : false,
  })
  if (response.status == 200) {
    await apiClient.put('/refundRequest/updateStatus', {
      refundRequestId: item.id,
      newRefundStatus: newStatus,
    })
  }
  return response.data
}

export const GetOrderTaxPercentage = async (orderId) => {
  const response = await apiClient.get(`/refundRequest/taxPercentage`, {
    params: { orderId },
  })
  return response.data
}
