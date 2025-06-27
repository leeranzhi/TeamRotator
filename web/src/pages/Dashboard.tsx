import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Stack
} from '@mui/material';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { format, parseISO } from 'date-fns';
import { api } from '../services/api';
import { Member, Task, TaskAssignment } from '../types';

const Dashboard: React.FC = () => {
  const queryClient = useQueryClient();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [selectedAssignment, setSelectedAssignment] = useState<TaskAssignment | null>(null);
  const [editForm, setEditForm] = useState({
    taskId: '',
    memberId: '',
    startDate: new Date(),
    endDate: new Date(),
  });

  const { data: assignments } = useQuery<TaskAssignment[]>({
    queryKey: ['assignments'],
    queryFn: () => api.get<TaskAssignment[]>('/assignments').then(res => res.data),
  });

  const { data: tasks } = useQuery<Task[]>({
    queryKey: ['tasks'],
    queryFn: () => api.get<Task[]>('/tasks').then(res => res.data),
  });

  const { data: members } = useQuery<Member[]>({
    queryKey: ['members'],
    queryFn: () => api.get<Member[]>('/members').then(res => res.data),
  });

  const updateAssignment = useMutation({
    mutationFn: (assignment: Partial<TaskAssignment>) =>
      api.put<TaskAssignment>(`/assignments/${selectedAssignment?.id}`, assignment).then(res => res.data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
      handleCloseDialog();
    },
  });

  const deleteAssignment = useMutation({
    mutationFn: (id: number) => api.delete(`/assignments/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] });
    },
  });

  const handleEdit = (assignment: TaskAssignment) => {
    setSelectedAssignment(assignment);
    setEditForm({
      taskId: assignment.taskId.toString(),
      memberId: assignment.memberId.toString(),
      startDate: parseISO(assignment.startDate.toString()),
      endDate: parseISO(assignment.endDate.toString()),
    });
    setEditDialogOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Are you sure you want to delete this assignment?')) {
      await deleteAssignment.mutateAsync(id);
    }
  };

  const handleCloseDialog = () => {
    setEditDialogOpen(false);
    setSelectedAssignment(null);
    setEditForm({
      taskId: '',
      memberId: '',
      startDate: new Date(),
      endDate: new Date(),
    });
  };

  const handleSave = async () => {
    await updateAssignment.mutateAsync({
      taskId: parseInt(editForm.taskId),
      memberId: parseInt(editForm.memberId),
      startDate: format(editForm.startDate, 'yyyy-MM-dd'),
      endDate: format(editForm.endDate, 'yyyy-MM-dd'),
    });
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Box p={3}>
        <Typography variant="h4" gutterBottom>
          Task Assignments Dashboard
        </Typography>
        <Grid container spacing={3}>
          {assignments?.map((assignment) => (
            <Grid item xs={12} sm={6} md={4} key={assignment.id}>
              <Card>
                <CardContent>
                  <Stack spacing={2}>
                    <Typography variant="h6" component="div">
                      {assignment.task.name}
                    </Typography>
                    <Typography color="text.secondary">
                      Assigned to: {assignment.member.name}
                    </Typography>
                    <Typography color="text.secondary">
                      Period: {format(parseISO(assignment.startDate.toString()), 'MMM dd, yyyy')} - 
                      {format(parseISO(assignment.endDate.toString()), 'MMM dd, yyyy')}
                    </Typography>
                    <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1 }}>
                      <IconButton onClick={() => handleEdit(assignment)} size="small">
                        <EditIcon />
                      </IconButton>
                      <IconButton onClick={() => handleDelete(assignment.id)} size="small" color="error">
                        <DeleteIcon />
                      </IconButton>
                    </Box>
                  </Stack>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>

        <Dialog open={editDialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
          <DialogTitle>Edit Assignment</DialogTitle>
          <DialogContent>
            <Stack spacing={3} sx={{ mt: 2 }}>
              <TextField
                select
                label="Task"
                value={editForm.taskId}
                onChange={(e) => setEditForm({ ...editForm, taskId: e.target.value })}
                fullWidth
              >
                {tasks?.map((task) => (
                  <MenuItem key={task.id} value={task.id}>
                    {task.name}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                label="Member"
                value={editForm.memberId}
                onChange={(e) => setEditForm({ ...editForm, memberId: e.target.value })}
                fullWidth
              >
                {members?.map((member) => (
                  <MenuItem key={member.id} value={member.id}>
                    {member.name}
                  </MenuItem>
                ))}
              </TextField>
              <DatePicker
                label="Start Date"
                value={editForm.startDate}
                onChange={(date) => date && setEditForm({ ...editForm, startDate: date })}
              />
              <DatePicker
                label="End Date"
                value={editForm.endDate}
                onChange={(date) => date && setEditForm({ ...editForm, endDate: date })}
              />
            </Stack>
          </DialogContent>
          <DialogActions>
            <Button onClick={handleCloseDialog}>Cancel</Button>
            <Button onClick={handleSave} variant="contained" color="primary">
              Save
            </Button>
          </DialogActions>
        </Dialog>
      </Box>
    </LocalizationProvider>
  );
};

export default Dashboard; 