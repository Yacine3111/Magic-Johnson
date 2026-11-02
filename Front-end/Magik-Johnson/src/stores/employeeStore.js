import { defineStore } from 'pinia'
import {
  AddEmployee,
  GetEmployeeById,
  ListEmployee,
  LockEmployee,
  UnLockEmployee,
  UpdateEmployee,
} from '@/api/employee/EmployeeApi'

export const useEmployeeStore = defineStore('Employee', {
  state: () => ({
    employeeSelected: null,
    listEmployees: null,
  }),

  actions: {
    async getSelectedEmployeeAsync(employeeId) {
      try {
        const employee = await GetEmployeeById(employeeId)
        this.employeeSelected = employee
      } catch (err) {
        this.employeeSelected = null
      }
    },
    async addEmployeeAsync(employee) {
      try {
        return await AddEmployee(employee)
      } catch (err) {
        throw err
      }
    },
    async updateEmployeeAsync(employee) {
      try {
        return await UpdateEmployee(employee)
      } catch (err) {
        throw err
      }
    },
    async lockEmployeeAsync(employeeId) {
      try {
        return await LockEmployee(employeeId)
      } catch (err) {
        throw err
      }
    },
    async unlockEmployeeAsync(employeeId) {
      try {
        return await UnLockEmployee(employeeId)
      } catch (err) {
        throw err
      }
    },
    async listEmployeeAsync() {
      try {
        this.listEmployees = await ListEmployee()
      } catch (err) {
        throw err
      }
    },
  },
})
