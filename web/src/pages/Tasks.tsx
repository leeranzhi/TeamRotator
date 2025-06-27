import React, { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  IconButton,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Stack,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Assignment as AssignmentIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { format } from 'date-fns';
import { api } from '../services/api';
import { Task, Member } from '../types';

interface AssignDialogProps {
  open: boolean;
  onClose: () => void;
  task: Task;
}

const AssignDialog: React.FC<AssignDialogProps> = ({ open, onClose, task }) => {
  const queryClient = useQueryClient();
  const [selectedMember, setSelectedMember] = useState<string>('');
  const [startDate, setStartDate] = useState<Date | null>(new Date());
  const [endDate, setEndDate] = useState<Date | null>(new Date());

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: () => api.get<Member[]>('/members').then((res) => res.data),
  });

  const assignMutation = useMutation({
    mutationFn: (data: { taskId: number; memberId: number; startDate: string; endDate: string }) =>
      api.post('/assignments', data).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
      onClose();
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedMember || !startDate || !endDate) return;

    assignMutation.mutate({
      taskId: task.id,
      memberId: parseInt(selectedMember),
      startDate: format(startDate, 'yyyy-MM-dd'),
      endDate: format(endDate, 'yyyy-MM-dd'),
    });
  };

  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>Assign Task: {task.name}</DialogTitle>
      <form onSubmit={handleSubmit}>
        <DialogContent>
          <Stack spacing={3}>
            <FormControl fullWidth>
              <InputLabel>Member</InputLabel>
              <Select
                value={selectedMember}
                onChange={(e) => setSelectedMember(e.target.value)}
                label="Member"
              >
                {members?.map((member) => (
                  <MenuItem key={member.id} value={member.id}>
                    {member.name} ({member.slackId})
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
            <LocalizationProvider dateAdapter={AdapterDateFns}>
              <DatePicker
                label="Start Date"
                value={startDate}
                onChange={(date) => setStartDate(date)}
              />
              <DatePicker
                label="End Date"
                value={endDate}
                onChange={(date) => setEndDate(date)}
              />
            </LocalizationProvider>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" color="primary">
            Assign
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};

const Tasks: React.FC = () => {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Partial<Task> | null>(null);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);

  const { data: tasks } = useQuery<Task[]>({
    queryKey: ['tasks'],
    queryFn: () => api.get<Task[]>('/tasks').then((res) => res.data),
  });

  const createMutation = useMutation({
    mutationFn: (task: Omit<Task, 'id'>) =>
      api.post<Task>('/tasks', task).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      handleClose();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, task }: { id: number; task: Partial<Task> }) =>
      api.put<Task>(`/tasks/${id}`, task).then((res) => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tasks'] });
      handleClose();
    },
  });

  const handleOpen = (task?: Task) => {
    setEditingTask(task || { name: '', description: '', rotationRule: '' });
    setOpen(true);
  };

  const handleClose = () => {
    setEditingTask(null);
    setOpen(false);
  };

  const handleAssign = (task: Task) => {
    setSelectedTask(task);
    setAssignDialogOpen(true);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingTask) return;

    if (editingTask.id) {
      updateMutation.mutate({
        id: editingTask.id,
        task: { name: editingTask.name, description: editingTask.description, rotationRule: editingTask.rotationRule },
      });
    } else {
      createMutation.mutate({
        name: editingTask.name!,
        description: editingTask.description!,
        rotationRule: editingTask.rotationRule!,
      });
    }
  };

  return (
    <Box p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Tasks</Typography>
        <Button
          variant="contained"
          color="primary"
          startIcon={<AddIcon />}
          onClick={() => handleOpen()}
        >
          Add Task
        </Button>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Description</TableCell>
              <TableCell>Rotation Rule</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {tasks?.map((task) => (
              <TableRow key={task.id}>
                <TableCell>{task.name}</TableCell>
                <TableCell>{task.description}</TableCell>
                <TableCell>{task.rotationRule}</TableCell>
                <TableCell align="right">
                  <IconButton onClick={() => handleAssign(task)} color="primary">
                    <AssignmentIcon />
                  </IconButton>
                  <IconButton onClick={() => handleOpen(task)}>
                    <EditIcon />
                  </IconButton>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      <Dialog open={open} onClose={handleClose}>
        <form onSubmit={handleSubmit}>
          <DialogTitle>{editingTask?.id ? 'Edit Task' : 'Add Task'}</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2 }}>
              <TextField
                autoFocus
                margin="dense"
                label="Task Name"
                fullWidth
                value={editingTask?.name || ''}
                onChange={(e) =>
                  setEditingTask((prev) => ({ ...prev!, name: e.target.value }))
                }
              />
              <TextField
                margin="dense"
                label="Description"
                fullWidth
                value={editingTask?.description || ''}
                onChange={(e) =>
                  setEditingTask((prev) => ({ ...prev!, description: e.target.value }))
                }
              />
              <TextField
                margin="dense"
                label="Rotation Rule"
                fullWidth
                value={editingTask?.rotationRule || ''}
                onChange={(e) =>
                  setEditingTask((prev) => ({ ...prev!, rotationRule: e.target.value }))
                }
              />
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleClose}>Cancel</Button>
            <Button type="submit" variant="contained" color="primary">
              {editingTask?.id ? 'Save' : 'Add'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {selectedTask && (
        <AssignDialog
          open={assignDialogOpen}
          onClose={() => {
            setAssignDialogOpen(false);
            setSelectedTask(null);
          }}
          task={selectedTask}
        />
      )}
    </Box>
  );
};

export default Tasks; 