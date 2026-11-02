import { apiClient } from '@/api/Axios'

export const GetEmployeeById = async (employeeId) => {
  const response = await apiClient.get(`/employee/get/${employeeId}`)
  return response.data
}

export const AddEmployee = async (employee) => {
  const response = await apiClient.post(`/employee/add`, employee)
  return response.data
}

export const UpdateEmployee = async (employee) => {
  const response = await apiClient.post(`/employee/update`, employee)
  return response.data
}

export const LockEmployee = async (employeeId) => {
  const response = await apiClient.post(`/employee/lockAccount/${employeeId}`)
  return response
}

export const UnLockEmployee = async (employeeId) => {
  const response = await apiClient.post(`/employee/unLockAccount/${employeeId}`)
  return response
}

export const ListEmployee = async () => {
  const response = await apiClient.get(`/employee/list`)
  return response.data
}
