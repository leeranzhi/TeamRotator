export interface Member {
  id: number;
  name: string;
  slackId: string;
}

export interface Task {
  id: number;
  name: string;
  description: string;
  rotationRule: string;
}

export interface TaskAssignment {
  id: number;
  taskId: number;
  memberId: number;
  startDate: string;
  endDate: string;
  task: Task;
  member: Member;
}

export interface Settings {
  id: number;
  slackWebhookUrl: string;
} 