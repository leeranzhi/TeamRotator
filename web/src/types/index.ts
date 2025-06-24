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
  memberId: number;
  task: Task;
  member: Member;
}

export interface Settings {
  id: number;
  slackWebhookUrl: string;
} 