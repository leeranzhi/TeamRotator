export interface Member {
  id: number;
  host: string;
  slackId: string;
}

export interface Task {
  id: number;
  taskName: string;
  rotationRule: string;
}

export interface TaskAssignment {
  id: number;
  taskId: number;
  taskName: string;
  memberId: number;
  host: string;
  slackId: string;
  startDate: string;
  endDate: string;
}

export interface ModifyAssignment {
  host: string;
  startDate: string;
  endDate: string;
}

export interface Settings {
  id: number;
  webhookUrl: string;
  personalWebhookUrl: string;
}

export interface TaskAssignmentDto {
  id: number;
  taskId: number;
  taskName: string;
  memberId: number;
  host: string;
  slackId: string;
  startDate: string;
  endDate: string;
} 