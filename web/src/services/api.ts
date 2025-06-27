import axios from 'axios';
import { Member, Task, TaskAssignment, Settings } from '../types';

export const api = axios.create({
  baseURL: 'http://localhost:5003/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add request interceptor for debugging
api.interceptors.request.use(
  (config) => {
    console.log('API Request:', config);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for debugging
api.interceptors.response.use(
  (response) => {
    console.log('API Response:', response);
    return response;
  },
  (error) => {
    console.error('API Response Error:', error);
    return Promise.reject(error);
  }
);

// Members
export const getMembers = async (): Promise<Member[]> => {
  const response = await api.get('/members');
  return response.data;
};

export const createMember = async (member: Omit<Member, 'id'>): Promise<Member> => {
  const response = await api.post('/members', member);
  return response.data;
};

export const updateMember = async (id: number, member: Partial<Member>): Promise<Member> => {
  const response = await api.put(`/members/${id}`, member);
  return response.data;
};

export const deleteMember = async (id: number): Promise<void> => {
  await api.delete(`/members/${id}`);
};

// Tasks
export const getTasks = async (): Promise<Task[]> => {
  const response = await api.get('/tasks');
  return response.data;
};

export const createTask = async (task: Omit<Task, 'id'>): Promise<Task> => {
  const response = await api.post('/tasks', task);
  return response.data;
};

export const updateTask = async (id: number, task: Partial<Task>): Promise<Task> => {
  const response = await api.put(`/tasks/${id}`, task);
  return response.data;
};

export const deleteTask = async (id: number): Promise<void> => {
  await api.delete(`/tasks/${id}`);
};

// Task Assignments
export const getAssignments = async (): Promise<TaskAssignment[]> => {
  const response = await api.get('/assignments');
  return response.data;
};

export const updateAssignment = async (
  taskId: number,
  assignment: { memberId: number; startDate: string; endDate: string }
): Promise<TaskAssignment> => {
  const response = await api.post(`/assignments/${taskId}/assign`, assignment);
  return response.data;
};

// Settings
export const getSettings = async (): Promise<Settings> => {
  const response = await api.get('/settings');
  return response.data;
};

export const updateSettings = async (settings: Partial<Settings>): Promise<Settings> => {
  const response = await api.put('/settings', settings);
  return response.data;
}; 